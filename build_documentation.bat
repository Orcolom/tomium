docfx docfx_project/docfx.json
DEL ".\UnityProject-Tomium\Packages\com.orcolom.tomium\Documentation~\" /Q
XCOPY "./docfx_project/_site" ".\UnityProject-Tomium\Packages\com.orcolom.tomium\Documentation~\" /Y /E
PAUSE