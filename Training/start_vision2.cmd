set config=vision_rnn_b64_seq64_lay1_hid128_mem256_beta1e-2_g990_ep3
start "%config%" cmd /c "activate mlagents & mlagents-learn %config%.yaml --env=.\FoosRLBuild_Agents64_Balls7\FoosRLBuild_Agents64_Balls7.exe --run-id=%config% --worker-id=2 --train & pause"
ping 127.0.0.1 -n 31 -w 1000 > nul