set config=nornn_b8192_g990_lr6e-5_beta1e-2
start "%config%" cmd /c "activate mlagents & mlagents-learn %config%.yaml --env=.\FoosRLBuild_Agents64\FoosRLBuild_Agents64.exe --run-id=%config% --worker-id=1 --train & pause"
ping 127.0.0.1 -n 31 -w 1000 > nul

set config=nornn_b8192_g990_lr6e-5_beta1e-1
start "%config%" cmd /c "activate mlagents & mlagents-learn %config%.yaml --env=.\FoosRLBuild_Agents64\FoosRLBuild_Agents64.exe --run-id=%config% --worker-id=2 --train & pause"
ping 127.0.0.1 -n 31 -w 1000 > nul


pause