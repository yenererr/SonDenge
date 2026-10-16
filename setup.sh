#!/usr/bin/env bash
# Kullanım: bash setup.sh <github-repo-url>
set -e

if [ ! -d "Assets" ] || [ ! -d "ProjectSettings" ]; then
  echo "Bu scripti Unity projesinin kök klasöründe çalıştırın (Assets ve ProjectSettings görünmeli)."
  exit 1
fi

command -v git >/dev/null || { echo "git kurulu değil."; exit 1; }
git lfs version >/dev/null 2>&1 || { echo "Git LFS kurulu değil: https://git-lfs.com"; exit 1; }

[ -d .git ] || git init -b main
git lfs install

git add .
git commit -m "Jam kit: başlangıç altyapısı" || echo "Commit edilecek değişiklik yok."

if [ -n "$1" ]; then
  git remote get-url origin >/dev/null 2>&1 || git remote add origin "$1"
  git push -u origin main
fi

echo "Hazır. Unity sürümü: $(head -n1 ProjectSettings/ProjectVersion.txt)"
