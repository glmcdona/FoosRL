import gym

from gym_unity.envs import UnityEnv
from baselines.common.vec_env.subproc_vec_env import SubprocVecEnv
from baselines.bench import Monitor
from baselines import logger
import baselines.ppo2.ppo2 as ppo2
import os

from random import randint

try:
    from mpi4py import MPI
except ImportError:
    MPI = None


def make_unity_env(env_directory, num_env, visual, start_index=0):
    """
    Create a wrapped, monitored Unity environment.
    """

    def make_env(rank, use_visual=True): # pylint: disable=C0111

        def _thunk():
            print("Making environment rank: %s" % rank)
            env = UnityEnv(env_directory, rank, use_visual=use_visual)
            env = Monitor(env, logger.get_dir() and os.path.join(logger.get_dir(), str(rank)))
            return env

        return _thunk

    if visual:
        env_fns = [make_env(i + start_index) for i in range(num_env)]
        print("Created environment functions. Count: %s" % len(env_fns))
        result = SubprocVecEnv(env_fns)
        print("Done.")
        return result
    else:
        env_fns = [make_env(i + start_index, visual) for i in range(num_env)]
        print("Created environment functions. Count: %s" % len(env_fns))
        result = SubprocVecEnv(env_fns)
        print("Done.")
        return result
        #rank = MPI.COMM_WORLD.Get_rank()
        #rank = 0
        #return make_env(rank, use_visual=False)


if __name__ == '__main__':
    env = make_unity_env("./FoosRLBuild_Agents1_Balls10/FoosRLBuild_Agents1_Balls10.exe", 15, False, randint(0,32767))

    ppo2.learn(
            network="mlp",
            env=env,
            total_timesteps=100000000,
            lr=1e-3,
            nsteps=6000,
            nminibatches=2,
            noptepochs = 3
        )
