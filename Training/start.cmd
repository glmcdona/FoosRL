set config=nornn_b512_g995_lr3e-4
start "%config%" cmd /c "activate mlagents & mlagents-learn %config%.yaml --env=.\FoosRLBuild_Agents64\FoosRLBuild_Agents64.exe --run-id=%config% --worker-id=0 --train & pause"
ping 127.0.0.1 -n 31 -w 1000 > nul

set config=nornn_b8192_g990_lr3e-4_beta1e-2
start "%config%" cmd /c "activate mlagents & mlagents-learn %config%.yaml --env=.\FoosRLBuild_Agents64\FoosRLBuild_Agents64.exe --run-id=%config% --worker-id=1 --train & pause"
ping 127.0.0.1 -n 31 -w 1000 > nul

set config=nornn_b8192_g995_lr3e-4
start "%config%" cmd /c "activate mlagents & mlagents-learn %config%.yaml --env=.\FoosRLBuild_Agents64\FoosRLBuild_Agents64.exe --run-id=%config% --worker-id=2 --train & pause"
ping 127.0.0.1 -n 31 -w 1000 > nul


pause