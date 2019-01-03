set config=simple
start "%config%" cmd /c "activate mlagents & mlagents-learn %config%.yaml --env=.\FoosRLBuild_Agents64_Balls7\FoosRLBuild_Agents64_Balls7.exe --run-id=%config% --worker-id=3 --train & pause"
ping 127.0.0.1 -n 31 -w 1000 > nul