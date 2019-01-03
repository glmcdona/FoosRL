set name=regular_vision
set worker=1

rm config_%worker%.yaml
copy template.yaml config_%worker%.yaml
echo     learning_rate: 1.0e-5 >> config_%worker%.yaml
echo     use_recurrent: false >> config_%worker%.yaml
echo     sequence_length: 64 >> config_%worker%.yaml
echo     num_layers: 1 >> config_%worker%.yaml
echo     hidden_units: 128 >> config_%worker%.yaml
echo     memory_size: 256 >> config_%worker%.yaml
echo     beta: 1.0e-2 >> config_%worker%.yaml
echo     lambd:  0.98 >> config_%worker%.yaml
echo     gamma: 0.999 >> config_%worker%.yaml
echo     num_epoch: 10 >> config_%worker%.yaml
echo     buffer_size: 256 >> config_%worker%.yaml
echo     batch_size: 32 >> config_%worker%.yaml
echo     max_steps: 5.0e10 >> config_%worker%.yaml
echo     summary_freq: 1000 >> config_%worker%.yaml
echo     time_horizon: 1024 >> config_%worker%.yaml
echo     epsilon:        0.2 >> config_%worker%.yaml


start "%name%" cmd /c "activate mlagents & mlagents-learn config_%worker%.yaml --env=.\FoosRLBuild_Agents64_Balls7\FoosRLBuild_Agents64_Balls7.exe --run-id=%config% --worker-id=%worker% --train & pause"
ping 127.0.0.1 -n 31 -w 1000 > nul