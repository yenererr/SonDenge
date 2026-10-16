# Kullanım: powershell -ExecutionPolicy Bypass -File setup.ps1 <github-repo-url>
param([string]$RepoUrl = "")

if (-not (Test-Path "Assets") -or -not (Test-Path "ProjectSettings")) {
    Write-Host "Bu scripti Unity projesinin kök klasöründe çalıştırın (Assets ve ProjectSettings görünmeli)."
    exit 1
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) { Write-Host "git kurulu değil."; exit 1 }
git lfs version *> $null
if ($LASTEXITCODE -ne 0) { Write-Host "Git LFS kurulu değil: https://git-lfs.com"; exit 1 }

if (-not (Test-Path ".git")) {
    git init *> $null
    git symbolic-ref HEAD refs/heads/main   # eski git sürümlerinde 'init -b' yok
}
git lfs install

git add .
git commit -m "Jam kit: başlangıç altyapısı"
if ($LASTEXITCODE -ne 0) { Write-Host "Commit edilecek değişiklik yok." }

if ($RepoUrl -ne "") {
    git remote get-url origin *> $null
    if ($LASTEXITCODE -ne 0) { git remote add origin $RepoUrl }
    git push -u origin main
    if ($LASTEXITCODE -ne 0) { Write-Host "Push başarısız. Repo URL'sini ve GitHub yetkinizi kontrol edin."; exit 1 }
}

Write-Host ("Hazır. Unity sürümü: " + (Get-Content ProjectSettings/ProjectVersion.txt -First 1))
