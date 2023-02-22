using Unity.Mathematics;

namespace PipelineV3.Maze
{
    public class MazeRoomDesignElement : DesignElement
    {
        public int2 lLPosition;
        public int width = 0, height = 0;
        public int2[] doorPositions;

        public MazeRoomDesignElement(MazeRoomDesignElement original, GenericLevel levelReference) : base(levelReference)
        {
            lLPosition = original.lLPosition;
            width = original.width;
            height = original.height;
            doorPositions = new int2[original.doorPositions.Length];
            original.doorPositions.CopyTo(doorPositions, 0);
        }

        public MazeRoomDesignElement(GenericLevel levelReference) : base(levelReference)
        {
        }   

        public override void Spawn()
        {
            levelReference.spawnEnvironment.Spawn("Room", this);
        }


        protected override void Mutate()
        {
        }

        public override DesignElement Clone(GenericLevel newOwner)
        {
            return new MazeRoomDesignElement(this, newOwner);
        }

        public override bool CheckValidity()
        {
            return true;
        }             
    }


    public class MazeWallDesignElement : DesignElement
    {

        public int startX, startY, length; //Coordinates
        public bool horizontal; //true = horizontal, false = vertical

        public MazeWallDesignElement(MazeWallDesignElement original, GenericLevel levelReference) : base(levelReference)
        {
            startX = original.startX;
            startY = original.startY;
            length = original.length;
            horizontal = original.horizontal;
        }

        public MazeWallDesignElement(GenericLevel levelReference) : base(levelReference)
        {
            startX = UnityEngine.Random.Range(0, MazeBuilderMetrics.WIDTH);
            startY = UnityEngine.Random.Range(0, MazeBuilderMetrics.HEIGHT);
            length = UnityEngine.Random.Range(0, MazeBuilderMetrics.MAX_WALL_LENGTH);
            //length = RandomLength();
            

            horizontal = UnityEngine.Random.Range(0f, 1f) > 0.5f;
        }

        public MazeWallDesignElement(GenericLevel levelReference, int x, int y, bool horizontal) : base(levelReference)
        {
            startX = x;
            startY = y;
            //length = RandomLength();
            length = UnityEngine.Random.Range(0, MazeBuilderMetrics.MAX_WALL_LENGTH);
            this.horizontal = horizontal;
        }

        public override void Spawn()
        {
            levelReference.spawnEnvironment.Spawn("Wall", this);
        }

        int RandomLength()
        {
            int count = 0;
            for (int i = 1; i <= MazeBuilderMetrics.MAX_WALL_LENGTH; i++)
                count += i;
            
            var r = UnityEngine.Random.Range(0, count);
            for (int i = 1; i <= MazeBuilderMetrics.MAX_WALL_LENGTH; i++)
            {
                r -= i;
                if(r <= 0)
                {
                    return MazeBuilderMetrics.MAX_WALL_LENGTH - i;
                }
            }
            return 1;
        }

        protected override void Mutate()
        {
            float r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_SHIFT_PROBABILITY)
            {
                int rX = UnityEngine.Random.Range(startX - 1, startX + 2); //max exclusive
                int rY = UnityEngine.Random.Range(startY - 1, startY + 2);
                if(rX >= 0 && rX < MazeBuilderMetrics.WIDTH)
                    startX = rX;
                if(rY >= 0 && rY < MazeBuilderMetrics.HEIGHT)
                    startY = rY;	
            }
            r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_FLIP_PROBABILITY)
                horizontal = !horizontal;

            r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_CHANGE_LENGTH_PROBABILITY)
            {
                length = UnityEngine.Random.Range(length - 1, length + 2);
                if(length <= 0)
                    length = 1;
            }
        }

        public override DesignElement Clone(GenericLevel newOwner)
        {
            return new MazeWallDesignElement(this, newOwner);
        }

        public override bool CheckValidity()
        {
            var offset = horizontal ? new int2(1,0) : new int2(0,1);
            var start = new int2(startX, startY);
            for (int i = 0; i < length; i++)
            {
                var oc = start + offset * i;
                if((oc.x == 0 && oc.y == 0) || (oc.x == (MazeBuilderMetrics.WIDTH - 1) && (oc.y == MazeBuilderMetrics.HEIGHT - 1)))
                    return false;
            }

            return true;
        }
    }

    public class OccupiedCellMazeDesignElement : DesignElement
    {
        public int x, y; //Coordinates

        public OccupiedCellMazeDesignElement(OccupiedCellMazeDesignElement original, GenericLevel levelReference) : base(levelReference)
        {
            x = original.x;
            y = original.y;
        }

        public OccupiedCellMazeDesignElement(GenericLevel levelReference) : base(levelReference)
        {
            x = UnityEngine.Random.Range(0, MazeBuilderMetrics.WIDTH);
            y = UnityEngine.Random.Range(0, MazeBuilderMetrics.HEIGHT);
        }

        public override void Spawn()
        {
            levelReference.spawnEnvironment.Spawn("OccupiedCell", this);
        }

        protected override void Mutate()
        {
            float rShift = UnityEngine.Random.Range(0f, 1f);
            if(rShift > MazeBuilderMetrics.TILE_SHIFT_PROPABILITY)
                return;
            int rX = UnityEngine.Random.Range(x - 1, x + 2); //max exclusive
            int rY = UnityEngine.Random.Range(y - 1, y + 2);
            if(rX >= 0 && rX < MazeBuilderMetrics.WIDTH)
                x = rX;
            if(rY >= 0 && rY < MazeBuilderMetrics.HEIGHT)
                y = rY;		

            //e.g. shift x and/or y coordinates by +1 or -1
        }

        public override DesignElement Clone(GenericLevel newOwner)
        {
            return new OccupiedCellMazeDesignElement(this, newOwner);
        }

        public override bool CheckValidity()
        {
            return !((x == 0 && y == 0) || (x == (MazeBuilderMetrics.WIDTH - 1) && y == (MazeBuilderMetrics.HEIGHT - 1)));
        }
    }
}