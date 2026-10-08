"""Inspect published PE imports without loading or executing any binary.

Usage: python Inspect-NativeDependencies.py PUBLISH_DIRECTORY REPORT.json
Exit 0: inspected files have no missing app-local VC runtime dependencies.
Exit 1: missing VC dependencies or malformed/unreadable PE files.
Exit 2: invalid arguments, directory, or report destination.

This is a static dependency inventory, NOT proof that the app runs on a clean
PC. It cannot observe dynamic LoadLibrary calls, COM activation, API availability,
or the loader's eventual search path. Ordinary Windows/API-set imports are listed
but not validated against this development computer's installed system libraries.
"""
import argparse
import json
import struct
import sys
from pathlib import Path


AUDIT_TYPE = "PublishedNativeDependencies"
SCOPE = (
    "Static PE normal/delay-import inventory only; not clean-PC execution proof. "
    "VC resolution checks the publish root and importing binary's directory, "
    "not PATH, System32, installed runtimes, or arbitrary publish subdirectories. "
    "Presence beside a subdirectory DLL still requires verification of its load path. "
    "Dynamic loading, COM activation and OS/API compatibility are not checked."
)
MACHINES = {0x14C: "x86", 0x8664: "x64", 0xAA64: "ARM64", 0xA641: "ARM64EC"}


class PeError(ValueError):
    pass


class PeReader:
    def __init__(self, data):
        self.data = data
        self.require(0, 64)
        if data[:2] != b"MZ":
            raise PeError("Missing DOS MZ signature")
        pe = self.number("I", 0x3C)
        self.require(pe, 24)
        if data[pe:pe + 4] != b"PE\0\0":
            raise PeError("Missing PE signature")
        self.machine = self.number("H", pe + 4)
        count = self.number("H", pe + 6)
        optional_size = self.number("H", pe + 20)
        optional = pe + 24
        self.require(optional, optional_size)
        magic = self.number("H", optional)
        if magic == 0x20B:
            self.format = "PE32+"
            directory_offset, directory_count = 112, 108
            self.image_base = self.number("Q", optional + 24)
        elif magic == 0x10B:
            self.format = "PE32"
            directory_offset, directory_count = 96, 92
            self.image_base = self.number("I", optional + 28)
        else:
            raise PeError(f"Unsupported optional-header magic 0x{magic:X}")
        if optional_size < directory_offset:
            raise PeError("Optional header is too short")
        self.header_size = self.number("I", optional + 60)
        self.directories = []
        for index in range(min(self.number("I", optional + directory_count), 16)):
            at = directory_offset + index * 8
            if at + 8 > optional_size:
                raise PeError("Data directories exceed optional header")
            self.directories.append(self.numbers("II", optional + at))
        self.sections = []
        self.require(optional + optional_size, count * 40)
        for index in range(count):
            self.sections.append(self.numbers("IIII", optional + optional_size + index * 40 + 8))

    def require(self, offset, size):
        if offset < 0 or size < 0 or offset + size > len(self.data):
            raise PeError("PE structure extends beyond file")

    def numbers(self, fmt, offset):
        self.require(offset, struct.calcsize("<" + fmt))
        return struct.unpack_from("<" + fmt, self.data, offset)

    def number(self, fmt, offset):
        return self.numbers(fmt, offset)[0]

    def offset(self, rva, size=1):
        if 0 <= rva < self.header_size:
            self.require(rva, size)
            return rva
        for virtual_size, virtual_address, raw_size, raw_offset in self.sections:
            delta = rva - virtual_address
            if 0 <= delta < max(virtual_size, raw_size):
                if delta + size > raw_size:
                    raise PeError("RVA points into unbacked section padding")
                self.require(raw_offset + delta, size)
                return raw_offset + delta
        raise PeError(f"RVA 0x{rva:X} is outside mapped sections")

    def directory(self, index):
        return self.directories[index] if index < len(self.directories) else (0, 0)

    def dll_name(self, rva):
        at = self.offset(rva)
        end = self.data.find(b"\0", at, min(len(self.data), at + 4096))
        if end < 0:
            raise PeError("Import DLL name is not terminated")
        try:
            name = self.data[at:end].decode("ascii")
        except UnicodeDecodeError as exc:
            raise PeError("Import DLL name is not ASCII") from exc
        if not name or any(value in name for value in ("/", "\\", ":")):
            raise PeError("Invalid import DLL name")
        return name

    def imports(self, delayed=False):
        rva, size = self.directory(13 if delayed else 1)
        if not rva:
            return []
        width = 32 if delayed else 20
        limit = min(65536, size // width if size else len(self.data) // width)
        result = []
        for index in range(limit):
            values = self.numbers("8I" if delayed else "5I", self.offset(rva + index * width, width))
            if not any(values):
                return sorted(set(result), key=str.casefold)
            if delayed:
                if values[0] & ~1:
                    raise PeError("Unsupported delay-import attributes")
                name_rva = values[1] if values[0] & 1 else values[1] - self.image_base
            else:
                name_rva = values[3]
            result.append(self.dll_name(name_rva))
        raise PeError("Import descriptor table has no terminating record")

    def managed_il_only(self):
        rva, _ = self.directory(14)
        if not rva:
            return False
        flags = self.number("I", self.offset(rva + 16, 4))
        return bool(flags & 1 and not flags & 2)


def is_vc_runtime(name):
    lowered = name.casefold()
    return lowered.endswith(".dll") and lowered.startswith(("vcruntime", "msvcp", "concrt", "vcomp"))


def inspect(directory):
    files = sorted((path for path in directory.rglob("*") if path.is_file()), key=lambda path: str(path).casefold())
    local_files = {str(path.relative_to(directory)).casefold(): path for path in files}
    binaries = [path for path in files if path.suffix.casefold() in (".exe", ".dll")]
    rows, errors, missing = [], [], []
    for path in binaries:
        relative = str(path.relative_to(directory))
        try:
            reader = PeReader(path.read_bytes())
            normal, delay = reader.imports(), reader.imports(delayed=True)
            vc = []
            for name in sorted(set(normal + delay), key=str.casefold):
                if not is_vc_runtime(name):
                    continue
                beside = str(path.parent.relative_to(directory) / name).casefold()
                resolved = local_files.get(name.casefold()) or local_files.get(beside)
                dependency = {"name": name, "importTypes": [kind for kind, entries in (("Normal", normal), ("Delay", delay)) if name in entries],
                    "appLocalPath": str(resolved.relative_to(directory)) if resolved else None,
                    "resolution": "PublishRoot" if resolved and resolved.parent == directory else "ImporterDirectoryOnly" if resolved else "Missing",
                    "possibleDebugRuntime": name.casefold().endswith("d.dll")}
                vc.append(dependency)
                if not resolved:
                    missing.append({"binary": relative, "dependency": name, "importTypes": dependency["importTypes"]})
            rows.append({"binary": relative, "peFormat": reader.format, "machine": MACHINES.get(reader.machine, f"0x{reader.machine:04X}"),
                "managedIlOnly": reader.managed_il_only(), "normalImports": normal, "delayImports": delay, "vcRuntimeDependencies": vc})
        except (OSError, PeError) as exc:
            errors.append({"binary": relative, "error": str(exc)})
    if not binaries:
        errors.append({"binary": None, "error": "No EXE/DLL files found in the publication directory"})
    return {"schemaVersion": 1, "auditType": AUDIT_TYPE, "scope": SCOPE,
        "publishDirectory": str(directory), "inspectedBinaryCount": len(rows), "candidateBinaryCount": len(binaries),
        "vcDependencyBinaryCount": sum(bool(row["vcRuntimeDependencies"]) for row in rows),
        "missingAppLocalVcDependencies": missing, "inspectionErrors": errors,
        "staticVcDependencyCheckPassed": not missing and not errors, "cleanPcExecutionProven": False, "binaries": rows}


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("publish_directory", type=Path)
    parser.add_argument("json_output", type=Path)
    args = parser.parse_args()
    directory, output = args.publish_directory.resolve(), args.json_output.resolve()
    if not directory.is_dir():
        parser.error("publish_directory must be an existing directory")
    if output.suffix.casefold() != ".json":
        parser.error("json_output must have a .json extension")
    if output.exists():
        try:
            previous = json.loads(output.read_text(encoding="utf-8-sig"))
            if not isinstance(previous, dict) or previous.get("auditType") != AUDIT_TYPE:
                parser.error("Existing unrelated output is preserved; choose a different report path")
        except (OSError, ValueError):
            parser.error("Existing unreadable or unrelated output is preserved")
    result = inspect(directory)
    try:
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    except OSError as exc:
        parser.error(f"Cannot write report: {exc}")
    passed = result["staticVcDependencyCheckPassed"]
    print(f"{'PASS' if passed else 'FAIL'}: {result['inspectedBinaryCount']} PE binaries inspected; "
        f"{len(result['missingAppLocalVcDependencies'])} missing app-local VC dependencies; {len(result['inspectionErrors'])} inspection errors.")
    print("Static inventory only; clean-PC execution is not proven.")
    return 0 if passed else 1


if __name__ == "__main__":
    sys.exit(main())
