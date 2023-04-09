# Creates the release's .zip file

$ModName = "Guil-科技至上";
$ModFolder = "./Package/" + $ModName
$ArchiveName = "Guil-Science.zip"

mkdir -ErrorAction SilentlyContinue $ModFolder
Remove-Item -ErrorAction SilentlyContinue -Recurse ./Package/*
Remove-Item -ErrorAction SilentlyContinue $ArchiveName

dotnet publish .\src\Rimworld.csproj -o $ModFolder -c Release

Copy-Item -Recurse "./$ModName/*" $ModFolder

# English name since github strips Unicode for security purposes.
Compress-Archive -DestinationPath $ArchiveName -Path ./Package/*

