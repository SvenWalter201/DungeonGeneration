namespace PipelineV3.Maze
{
    public static class MazeBuilderMetrics
    {
        public static int GetIndex(int x, int y) => y * WIDTH + x;
        public static int CellAmount => WIDTH * HEIGHT;
        public static int WIDTH = 30, HEIGHT = 30;
        public static int POP_SIZE = 40;
        public static int MAX_GENERATIONS = 2000;
        public static int MAX_CROSSOVERS = 10;
        public static float 
            START_FILL_PERCENTAGE = 0.5f,//0.5f, 
            TILE_SHIFT_PROPABILITY = 0.02f,//0.002f,
            TILE_ADD_PROPABILITY = 0.8f,//0.1f,
            TILE_REMOVE_PROPABILITY = 0.2f;//0.02f;


        public static int START_WALL_AMOUNT = 0;//10;
        public static int MAX_WALL_LENGTH = 25;
        public static float 
            WALL_ADD_PROPABILITY = 0.0f,//0.8f,//0.2f, 
            WALL_REMOVE_PROPABILITY = 0.3f,//0.02f, 
            WALL_CHANGE_LENGTH_PROBABILITY = 0.04f,//0.01f,
            WALL_FLIP_PROBABILITY = 0.04f,//0.01f, 
            WALL_SHIFT_PROBABILITY = 0.04f;//0.01f;
    }
}