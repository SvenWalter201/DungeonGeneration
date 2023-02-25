namespace PipelineV3.Maze
{
    public static class MazeBuilderMetrics
    {

        public static int GetIndex(int x, int y) => y * WIDTH + x;
        public static int CellAmount => WIDTH * HEIGHT;
        public static int WIDTH = 0, HEIGHT = 0;
        public static int POP_SIZE = 0;
        public static int MAX_GENERATIONS = 0;
        public static int MAX_CROSSOVERS = 0;


        //FITNESS
        public static int OPTIMAL_PATH_LENGTH = 150, OPTIMAL_CUL_DE_SAC_LENGTH = 15;
        public static float OPTIMAL_FILL_PERCENTAGE = 0.8f;

        //CELLS
        public static float 
            START_FILL_PERCENTAGE = 0.0f,//0.5f, 
            TILE_SHIFT_PROBABILITY = 0.0f,//0.002f,
            TILE_ADD_PROBABILITY = 0.0f,//0.1f,
            TILE_REMOVE_PROBABILITY = 0.0f;//0.02f;

        //WALLS
        public static int START_WALL_AMOUNT = 0;//10;
        public static int MAX_WALL_LENGTH = 0;
        public static float 
            WALL_ADD_PROBABILITY = 0.0f,//0.8f,//0.2f, 
            WALL_REMOVE_PROBABILITY = 0.0f,//0.02f, 
            WALL_CHANGE_LENGTH_PROBABILITY = 0.0f,//0.01f,
            WALL_FLIP_PROBABILITY = 0.0f,//0.01f, 
            WALL_SHIFT_PROBABILITY = 0.0f;//0.01f;

        //ROOMS
        public static int START_ROOM_AMOUNT = 0;
        public static int MIN_ROOM_SIZE = 0, MAX_ROOM_SIZE = 0;
        public static int MAX_DOOR_AMOUNT = 0;

        public static float 
            ROOM_ADD_PROBABILITY = 0.0f,
            ROOM_REMOVE_PROBABILITY = 0.0f,
            ROOM_CHANGE_SIZE_PROBABILITY = 0.0f,
            ROOM_SHIFT_PROBABILITY = 0.0f,
            ROOM_CHANGE_DOOR_PROBABILITY = 0.0f;

    }
}