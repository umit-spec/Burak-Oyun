#!/usr/bin/env python3
"""
BurakOyun — CI Statik Doğrulayıcı
Unity gerektirmez; dosya sistemi + manifest üzerinden kontrol yapar.
Çıkış kodu: 0 = tüm kontroller geçti, 1 = en az bir FAIL var.
"""

import os
import sys
import glob

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
_results: list[tuple[bool, str, str]] = []


def chk(label: str, ok: bool, fix: str = "") -> None:
    _results.append((ok, label, fix))
    if ok:
        print(f"  ✅ PASS  {label}")
    else:
        print(f"  ❌ FAIL  {label}")
        if fix:
            print(f"           → {fix}")


def exists(rel: str) -> bool:
    return os.path.isfile(os.path.join(ROOT, rel))


def has_assets(rel: str, pattern: str = "*.asset") -> bool:
    d = os.path.join(ROOT, rel)
    return os.path.isdir(d) and bool(glob.glob(os.path.join(d, pattern)))


def read(rel: str) -> str:
    p = os.path.join(ROOT, rel)
    try:
        return open(p, encoding="utf-8").read()
    except OSError:
        return ""


BAR = "═" * 56


def main() -> int:
    print(BAR)
    print("  BurakOyun — CI Static Validation")
    print(BAR)
    print()

    # ── 1. Sahne dosyası ──────────────────────────────────────────────
    print("[ Sahne ]")
    chk(
        "Assets/Scenes/Game.unity mevcut",
        exists("Assets/Scenes/Game.unity"),
        "Unity'de: BurakOyun → 3 — Sahneyi Kur",
    )

    build_cfg = read("ProjectSettings/EditorBuildSettings.asset")
    chk(
        "Game.unity Build Settings içinde kayıtlı",
        "Game.unity" in build_cfg,
        "BurakOyun → 3 — Sahneyi Kur (otomatik ekler); ardından ProjectSettings/ commit et",
    )

    # ── 2. Render pipeline ───────────────────────────────────────────
    print()
    print("[ Render Pipeline ]")
    chk(
        "URP Pipeline asset (Assets/Settings/URP-Pipeline.asset)",
        exists("Assets/Settings/URP-Pipeline.asset"),
        "Unity'de: BurakOyun → 0 — URP Pipeline Kur",
    )

    manifest = read("Packages/manifest.json")
    chk(
        "manifest.json: com.unity.render-pipelines.universal",
        "render-pipelines.universal" in manifest,
        "Packages/manifest.json dosyasını kontrol et",
    )

    # ── 3. TextMeshPro ───────────────────────────────────────────────
    print()
    print("[ TextMeshPro ]")
    chk(
        "TMP Essential Resources import edilmiş",
        has_assets("Assets/TextMesh Pro/Resources/Fonts & Materials"),
        "Unity'de: Window → TextMeshPro → Import TMP Essential Resources",
    )

    chk(
        "manifest.json: com.unity.textmeshpro",
        "textmeshpro" in manifest,
        "Packages/manifest.json dosyasını kontrol et",
    )

    # ── 4. Android Player Settings ───────────────────────────────────
    print()
    print("[ Android Player Settings ]")
    ps = read("ProjectSettings/ProjectSettings.asset")

    chk(
        "Scripting Backend: IL2CPP",
        "IL2CPP" in ps,
        "Unity'de: BurakOyun → Android — Player Ayarlarını Kur",
    )

    chk(
        "Target Architecture: ARM64",
        "ARM64" in ps,
        "Unity'de: BurakOyun → Android — Player Ayarlarını Kur",
    )

    chk(
        "Paket adı: com.burakoyun.*",
        "com.burakoyun" in ps,
        "Unity'de: BurakOyun → Android — Player Ayarlarını Kur",
    )

    # ── 5. asmdef referansları ───────────────────────────────────────
    print()
    print("[ Assembly Definitions ]")
    asmdef = read("Assets/Scripts/BurakOyun.Runtime.asmdef")

    chk(
        "Runtime asmdef: Unity.InputSystem referansı",
        "Unity.InputSystem" in asmdef,
        "Assets/Scripts/BurakOyun.Runtime.asmdef",
    )

    chk(
        "Runtime asmdef: Unity.TextMeshPro referansı",
        "Unity.TextMeshPro" in asmdef,
        "Assets/Scripts/BurakOyun.Runtime.asmdef",
    )

    # ── Özet ──────────────────────────────────────────────────────────
    passed = sum(1 for ok, *_ in _results if ok)
    failed = sum(1 for ok, *_ in _results if not ok)
    total = len(_results)

    print()
    print(BAR)
    print(f"  Sonuç: {passed}/{total} PASS   {failed} FAIL")
    print(BAR)

    if failed:
        print()
        print("  Başarısız kontroller ve düzeltme adımları:")
        print()
        for ok, label, fix in _results:
            if not ok:
                print(f"    ❌ {label}")
                if fix:
                    print(f"       → {fix}")
        print()
        print("  ProjectSettings/ klasörünü Unity'de kurulum sonrası commit etmeyi unutma.")
        return 1

    return 0


if __name__ == "__main__":
    sys.exit(main())
