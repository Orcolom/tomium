RMDIR "%cd%\UnityProject-Tomium\Packages\com.orcolom.tomium\Samples~\" /Q/S

XCOPY ".\UnityProject-Tomium\Assets\Samples\Tomium\Latest" ".\UnityProject-Tomium\Packages\com.orcolom.tomium\Samples~\" /Y /E
PAUSE