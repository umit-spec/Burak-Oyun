#!/usr/bin/env python3
"""
BurakOyun — CI Statik Doğrulayıcı
Unity gerektirmez; dosya sistemi + manifest kontrolü.
Çıkış kodu: 0 = PASS, 1 = en az bir FAIL var.
"""

import os
import sys
import glob
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
_results: list[tuple[bool, str, str, str]] = []   # (ok, label, fix, git_cmd)


# ── Yardımcılar ───────────────────────────────────────────────────────────────

def chk(label: str, ok: bool, fix: str = "", git_cmd: str = "") -> None:
    _results.append((ok, label, fix, git_cmd))
    if ok:
        print(f"  ✅  {label}")
    else:
        print(f"  ❌  {label}")
        if fix:
            print(f"        Düzelt : {fix}")
        if git_cmd:
            print(f"        Git    : {git_cmd}")


def exists(rel: str) -> bool:
    return (ROOT / rel).is_file()


def has_assets(rel: str, pattern: str = "*.asset") -> bool:
    d = ROOT / rel
    return d.is_dir() and bool(list(d.glob(pattern)))


def read(rel: str) -> str:
    try:
        return (ROOT / rel).read_text(encoding="utf-8")
    except OSError:
        return ""


def meta_ok(*rels: str) -> bool:
    """Verilen kaynak dosyaların .meta kardeşleri commit edilmiş mi?"""
    return all(exists(r + ".meta") for r in rels)


BAR = "═" * 60


# ── Kontroller ────────────────────────────────────────────────────────────────

def main() -> int:
    print(BAR)
    print("  BurakOyun — CI Static Validation")
    print(BAR)

    # ── 1. ProjectSettings ────────────────────────────────────────────
    print()
    print("[ 1 / ProjectSettings ]")
    print("  Unity projeyi ilk açtığında bu klasörü üretir; tümü commit edilmeli.")

    chk("ProjectVersion.txt mevcut",
        exists("ProjectSettings/ProjectVersion.txt"),
        "Unity Hub'dan projeyi aç → Unity oluşturur",
        "git add ProjectSettings/ && git commit -m 'Add ProjectSettings'")

    chk("ProjectSettings.asset mevcut",
        exists("ProjectSettings/ProjectSettings.asset"),
        "Unity Hub'dan projeyi aç → Unity oluşturur; ardından BurakOyun → Android — Player Ayarlarını Kur",
        "git add ProjectSettings/ && git commit -m 'Add ProjectSettings'")

    ps = read("ProjectSettings/ProjectSettings.asset")

    chk("  ↳ Scripting Backend: IL2CPP",
        "IL2CPP" in ps,
        "Unity'de: BurakOyun → Android — Player Ayarlarını Kur",
        "git add ProjectSettings/ProjectSettings.asset && git commit -m 'Android: IL2CPP + ARM64'")

    chk("  ↳ Target Architecture: ARM64",
        "ARM64" in ps,
        "Unity'de: BurakOyun → Android — Player Ayarlarını Kur",
        "git add ProjectSettings/ProjectSettings.asset && git commit -m 'Android: IL2CPP + ARM64'")

    chk("  ↳ Paket adı com.burakoyun.*",
        "com.burakoyun" in ps,
        "Unity'de: BurakOyun → Android — Player Ayarlarını Kur",
        "git add ProjectSettings/ProjectSettings.asset && git commit -m 'Android: set package name'")

    chk("EditorBuildSettings.asset mevcut ve Game.unity kayıtlı",
        "Game.unity" in read("ProjectSettings/EditorBuildSettings.asset"),
        "Unity'de: BurakOyun → 3 — Sahneyi Kur (otomatik ekler)",
        "git add ProjectSettings/EditorBuildSettings.asset && git commit -m 'Add Game.unity to build settings'")

    # ── 2. Sahne ──────────────────────────────────────────────────────
    print()
    print("[ 2 / Sahne ]")

    chk("Assets/Scenes/Game.unity commit edilmiş",
        exists("Assets/Scenes/Game.unity"),
        "Unity'de: BurakOyun → 3 — Sahneyi Kur",
        "git add Assets/Scenes/ && git commit -m 'Add Game scene'")

    chk("Assets/Scenes/Game.unity.meta commit edilmiş",
        exists("Assets/Scenes/Game.unity.meta"),
        "Game.unity oluşturulduktan sonra .meta otomatik üretilir; commit et",
        "git add Assets/Scenes/ && git commit -m 'Add scene .meta'")

    # ── 3. URP Pipeline ───────────────────────────────────────────────
    print()
    print("[ 3 / URP Pipeline ]")

    chk("Assets/Settings/URP-Pipeline.asset commit edilmiş",
        exists("Assets/Settings/URP-Pipeline.asset"),
        "Unity'de: BurakOyun → 0 — URP Pipeline Kur",
        "git add Assets/Settings/ && git commit -m 'Add URP pipeline asset'")

    chk("Assets/Settings/URP-Pipeline.asset.meta commit edilmiş",
        exists("Assets/Settings/URP-Pipeline.asset.meta"),
        "Asset oluşturulduktan sonra .meta otomatik üretilir; commit et",
        "git add Assets/Settings/ && git commit -m 'Add URP .meta'")

    chk("manifest.json: com.unity.render-pipelines.universal",
        "render-pipelines.universal" in read("Packages/manifest.json"),
        "Packages/manifest.json dosyasını kontrol et",
        "git add Packages/ && git commit -m 'Fix manifest'")

    # ── 4. TextMeshPro ────────────────────────────────────────────────
    print()
    print("[ 4 / TextMeshPro ]")

    chk("TMP Essential Resources commit edilmiş",
        has_assets("Assets/TextMesh Pro/Resources/Fonts & Materials"),
        "Unity'de: Window → TextMeshPro → Import TMP Essential Resources",
        "git add 'Assets/TextMesh Pro/' && git commit -m 'Import TMP Essential Resources'")

    chk("manifest.json: com.unity.textmeshpro",
        "textmeshpro" in read("Packages/manifest.json"),
        "Packages/manifest.json dosyasını kontrol et",
        "git add Packages/ && git commit -m 'Fix manifest'")

    # ── 5. .meta dosyaları ────────────────────────────────────────────
    print()
    print("[ 5 / .meta Dosyaları ]")
    print("  Her Unity asset'in .meta kardeşi commit edilmeli (GUID tutarlılığı için).")

    # Repoda var olan kaynak dosyalar için .meta kontrolü
    chk("Scripts .meta commit edilmiş  (GameManager.cs.meta)",
        meta_ok("Assets/Scripts/Core/GameManager.cs"),
        "Unity proje ilk açıldığında .meta üretilir; commit et",
        "git add Assets/Scripts/ && git commit -m 'Add .meta files for scripts'")

    chk("asmdef .meta commit edilmiş  (BurakOyun.Runtime.asmdef.meta)",
        meta_ok("Assets/Scripts/BurakOyun.Runtime.asmdef"),
        "Unity proje ilk açıldığında .meta üretilir; commit et",
        "git add Assets/Scripts/ && git commit -m 'Add asmdef .meta'")

    chk("Editor scripts .meta commit edilmiş  (SceneSetup.cs.meta)",
        meta_ok("Assets/Editor/SceneSetup.cs"),
        "Unity proje ilk açıldığında .meta üretilir; commit et",
        "git add Assets/Editor/ && git commit -m 'Add editor .meta files'")

    chk("Test scripts .meta commit edilmiş  (WordProgressTests.cs.meta)",
        meta_ok("Assets/Tests/EditMode/WordProgressTests.cs"),
        "Unity proje ilk açıldığında .meta üretilir; commit et",
        "git add Assets/Tests/ && git commit -m 'Add test .meta files'")

    # ── 6. Assembly Definitions ───────────────────────────────────────
    print()
    print("[ 6 / Assembly Definitions ]")

    asmdef = read("Assets/Scripts/BurakOyun.Runtime.asmdef")

    chk("Runtime asmdef: Unity.InputSystem referansı",
        "Unity.InputSystem" in asmdef,
        "Assets/Scripts/BurakOyun.Runtime.asmdef dosyasını düzelt",
        "git add Assets/Scripts/BurakOyun.Runtime.asmdef && git commit -m 'Fix asmdef refs'")

    chk("Runtime asmdef: Unity.TextMeshPro referansı",
        "Unity.TextMeshPro" in asmdef,
        "Assets/Scripts/BurakOyun.Runtime.asmdef dosyasını düzelt",
        "git add Assets/Scripts/BurakOyun.Runtime.asmdef && git commit -m 'Fix asmdef refs'")

    # ── Özet ──────────────────────────────────────────────────────────
    passed = sum(1 for r in _results if r[0])
    failed = sum(1 for r in _results if not r[0])
    total = len(_results)

    print()
    print(BAR)
    print(f"  Sonuç: {passed} / {total} PASS   —   {failed} FAIL")
    print(BAR)

    if failed:
        _print_quick_fix()
        return 1

    print()
    print("  Tüm kontroller geçti. ▶ Android build başlayabilir.")
    return 0


def _print_quick_fix() -> None:
    print()
    print("  Başarısız kontroller:")
    for ok, label, fix, git_cmd in _results:
        if not ok:
            label_clean = label.lstrip(" ↳")
            print(f"    ❌  {label_clean.strip()}")
            if fix:
                print(f"        Düzelt : {fix}")
            if git_cmd:
                print(f"        Git    : {git_cmd}")

    # Kategorize et: ProjectSettings mi yoksa asset mi eksik?
    missing_ps   = any(not ok and "ProjectSettings" in label for ok, label, *_ in _results)
    missing_meta = any(not ok and ".meta" in label for ok, label, *_ in _results)
    missing_asset = any(not ok and ("Game.unity" in label or "URP" in label or "TMP" in label)
                        for ok, label, *_ in _results)

    print()
    print("  ── Hızlı Düzeltme ──────────────────────────────────────────────")
    step = 1
    print(f"  {step}. Unity Hub → projeyi aç (ilk açılışta paketler indirilir, ~3-5 dk)")
    step += 1
    if missing_asset or missing_meta:
        print(f"  {step}. Unity editörde kurulum menülerini çalıştır:")
        print(f"       BurakOyun → 0 — URP Pipeline Kur")
        print(f"       Window → TextMeshPro → Import TMP Essential Resources")
        print(f"       BurakOyun → 3 — Sahneyi Kur")
        print(f"       BurakOyun → Android — Player Ayarlarını Kur")
        step += 1
    print(f"  {step}. Git'e ekle ve commit et:")
    print(f"       git add ProjectSettings/ Assets/ Packages/")
    print(f"       git commit -m 'Unity setup: ProjectSettings + sahne + assets + .meta'")
    print(f"       git push")
    print()
    print("  Bundan sonra CI otomatik tetiklenecek ve PASS almalı.")


if __name__ == "__main__":
    sys.exit(main())
