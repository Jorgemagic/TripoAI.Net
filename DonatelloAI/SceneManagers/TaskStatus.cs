﻿namespace DonatelloAI.SceneManagers
{
    public class TaskStatus
    {
        public enum TaskType
        {
            Refine,
            PreRigCheck,
            Rig,
            Retarget,
            Lego,
            Voxel,
            Voronoi,
        }

        public string TaskId;
        public TaskType Type;
        public string ModelName;
        public int progress;
        public string msg;
    }
}
