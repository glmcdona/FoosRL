# FoosRL
Reinforcement learning physics model for Foosball using ml-agents.

It's recommended to use tensorflow-gpu edition because this is a vision deep-learning model.

Setup:
* mlagents v0.15.0
** Follow the regular ml-agents installation guide https://github.com/Unity-Technologies/ml-agents/blob/latest_release/docs/Installation.md, and follow the "Installation for Development" instructions for cloning and installing ml-agents yourself.
** Modify \ml-agents\setup.py, changing "tensorflow" requirement to "tensorflow-gpu"
** Install it in your environment by "pip install -e .". It's now set up to use your gpu instead.



