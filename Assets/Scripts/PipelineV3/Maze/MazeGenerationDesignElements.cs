using Unity.Mathematics;
using System.Collections.Generic;

namespace PipelineV3.Maze
{
    public class MazeRoomDesignElement : DesignElement
    {
        public int2 lLPosition;
        public int width = 0, height = 0;
        public List<int2> doorPositions;

        public MazeRoomDesignElement(MazeRoomDesignElement original, GenericLevel levelReference) : base(levelReference)
        {
            lLPosition = original.lLPosition;
            width = original.width;
            height = original.height;
            doorPositions = new List<int2>(original.doorPositions.Count);
            doorPositions.AddRange(original.doorPositions);
        }

        public MazeRoomDesignElement(GenericLevel levelReference) : base(levelReference)
        {
            width = UnityEngine.Random.Range(MazeBuilderMetrics.MIN_ROOM_SIZE, MazeBuilderMetrics.MAX_ROOM_SIZE + 1);            
            height = UnityEngine.Random.Range(MazeBuilderMetrics.MIN_ROOM_SIZE, MazeBuilderMetrics.MAX_ROOM_SIZE + 1);

            lLPosition = new int2(
                UnityEngine.Random.Range(0, MazeBuilderMetrics.WIDTH - (width - 1)),
                UnityEngine.Random.Range(1, MazeBuilderMetrics.HEIGHT - height - 1));

            


            var doorAmount = UnityEngine.Random.Range(1, MazeBuilderMetrics.MAX_DOOR_AMOUNT + 1);
            doorPositions = new List<int2>(doorAmount);
            for (int i = 0; i < 1000 && doorPositions.Count < doorAmount; i++)
            {
                var horizontal = UnityEngine.Random.Range(0f, 1f) < 0.5f;
                var offsetted = UnityEngine.Random.Range(0f, 1f) < 0.5f;
                var offset = (offsetted && horizontal) ? new int2(0, height - 1) : ((offsetted && !horizontal) ? new int2(width - 1, 0) : int2.zero);
                var direction = horizontal ? new int2(1,0) : new int2(0,1);
                var maxOffset = horizontal ? width : height;

                var doorPosition = lLPosition + offset + direction * UnityEngine.Random.Range(1, maxOffset - 1);
                bool identialDoorFound = false;
                foreach (var door in doorPositions)
                {
                    if(door.x == doorPosition.x && door.y == doorPosition.y)
                    {
                        identialDoorFound = true;
                        break;
                    }
                }
                if(identialDoorFound)
                    continue;

                doorPositions.Add(doorPosition);
            }
        }  

        public bool Overlap(MazeRoomDesignElement other)
        {
            var lhsURPosition = lLPosition + new int2(width, height);
            var otherURPosition = other.lLPosition + new int2(other.width, other.height);

            bool widthPositive = math.min(lhsURPosition.x, otherURPosition.x) > math.max(lLPosition.x, other.lLPosition.x);
            bool heightPositive = math.min(lhsURPosition.y, otherURPosition.y) > math.max(lLPosition.y, other.lLPosition.y);
            return (widthPositive && heightPositive);
        } 

        public override void Spawn()
        {
            levelReference.spawnEnvironment.Spawn("Room", this);
        }

        protected override bool Mutate()
        {
            bool mutationOccured = false;
            float r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.ROOM_SHIFT_PROBABILITY)
            {
                int rX = UnityEngine.Random.Range(lLPosition.x - 1, lLPosition.y + 2); //max exclusive
                int rY = UnityEngine.Random.Range(lLPosition.y - 1, lLPosition.y + 2);
                if(rX >= 1 && rX < MazeBuilderMetrics.WIDTH - (width - 1) && rX != lLPosition.x)
                {
                    lLPosition.x = rX;
                    mutationOccured = true;
                }
                if(rY >= 1 && rY < MazeBuilderMetrics.HEIGHT - (height - 1) && rY != lLPosition.y)
                {
                    lLPosition.y = rY;
                    mutationOccured = true;
                }
            }
            r = UnityEngine.Random.Range(0f, 1f);

            if(r < MazeBuilderMetrics.ROOM_CHANGE_SIZE_PROBABILITY)
            {
                int rWidth = UnityEngine.Random.Range(width - 1, width + 2); //max exclusive
                int rHeight = UnityEngine.Random.Range(height - 1, height + 2);
                if(rWidth >= MazeBuilderMetrics.MIN_ROOM_SIZE && rWidth <= MazeBuilderMetrics.MAX_ROOM_SIZE && rWidth != width)
                {
                    width = rWidth;
                    mutationOccured = true;
                }
                if(rHeight >= MazeBuilderMetrics.MIN_ROOM_SIZE && rHeight <= MazeBuilderMetrics.MAX_ROOM_SIZE && rHeight != height)
                {
                    height = rHeight;
                    mutationOccured = true;
                }
            }
            
            r = UnityEngine.Random.Range(0f, 1f);

            if(r < MazeBuilderMetrics.ROOM_CHANGE_DOOR_PROBABILITY)
            {
                int doorIndex = UnityEngine.Random.Range(0, doorPositions.Count);
                for (int i = 0; i < 100; i++)
                {
                    var horizontal = UnityEngine.Random.Range(0f, 1f) < 0.5f;
                    var offsetted = UnityEngine.Random.Range(0f, 1f) < 0.5f;
                    var offset = (offsetted && horizontal) ? new int2(0, height - 1) : ((offsetted && !horizontal) ? new int2(width - 1, 0) : int2.zero);
                    var direction = horizontal ? new int2(1,0) : new int2(0,1);
                    var maxOffset = horizontal ? width : height;

                    var doorPosition = lLPosition + offset + direction * UnityEngine.Random.Range(1, maxOffset - 1);
                    bool identialDoorFound = false;
                    foreach (var door in doorPositions)
                    {
                        if(door.x == doorPosition.x && door.y == doorPosition.y)
                        {
                            identialDoorFound = true;
                            break;
                        }
                    }
                    if(identialDoorFound)
                        continue;

                    doorPositions[doorIndex] = doorPosition;
                    mutationOccured = true;
                    break;
                }
            }

            return mutationOccured;
        }

        public override DesignElement Clone(GenericLevel newOwner)
        {
            return new MazeRoomDesignElement(this, newOwner);
        }

        public override bool CheckValidity()
        {
            var rooms = levelReference.GetDesignElementsOfType<MazeRoomDesignElement>();
            if(rooms == null)
                return true;

            foreach (var room in rooms)
            {
                if(Overlap(room))
                    return false;
            }
            return true;
        }             
    }


    public class MazeWallDesignElement : DesignElement
    {
        public int2 startPosition;
        public int length; //Coordinates
        public bool horizontal; //true = horizontal, false = vertical

        public MazeWallDesignElement(MazeWallDesignElement original, GenericLevel levelReference) : base(levelReference)
        {
            startPosition = original.startPosition;
            length = original.length;
            horizontal = original.horizontal;
        }

        public MazeWallDesignElement(GenericLevel levelReference) : base(levelReference)
        {
            startPosition = new int2(
                UnityEngine.Random.Range(0, MazeBuilderMetrics.WIDTH),
                UnityEngine.Random.Range(0, MazeBuilderMetrics.HEIGHT));
            length = UnityEngine.Random.Range(1, MazeBuilderMetrics.MAX_WALL_LENGTH);
            //length = RandomLength();
            

            horizontal = UnityEngine.Random.Range(0f, 1f) > 0.5f;
        }

        public MazeWallDesignElement(GenericLevel levelReference, int2 xy, bool horizontal) : base(levelReference)
        {
            startPosition = xy;
            //length = RandomLength();
            length = UnityEngine.Random.Range(1, MazeBuilderMetrics.MAX_WALL_LENGTH);
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

        protected override bool Mutate()
        {
            bool mutationOccured = false;
            float r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_SHIFT_PROBABILITY)
            {
                int rX = UnityEngine.Random.Range(startPosition.x - 1, startPosition.y + 2); //max exclusive
                int rY = UnityEngine.Random.Range(startPosition.y - 1, startPosition.y + 2);
                if(rX >= 0 && rX < MazeBuilderMetrics.WIDTH && rX != startPosition.x)
                {
                    startPosition.x = rX;
                    mutationOccured = true;
                }
                if(rY >= 0 && rY < MazeBuilderMetrics.HEIGHT && rY != startPosition.y)
                {
                    startPosition.y = rY;	
                    mutationOccured = true;
                }
            }
            r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_FLIP_PROBABILITY)
            {
                horizontal = !horizontal;
                mutationOccured = true;
            }

            r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_CHANGE_LENGTH_PROBABILITY)
            {
                var newLength = UnityEngine.Random.Range(length - 1, length + 2);
                if(newLength > 0 && newLength <= MazeBuilderMetrics.MAX_WALL_LENGTH && newLength != length)
                {
                    length = newLength;
                    mutationOccured = true;
                }
            }

            return mutationOccured;
        }

        public override DesignElement Clone(GenericLevel newOwner)
        {
            return new MazeWallDesignElement(this, newOwner);
        }

        public override bool CheckValidity()
        {
            var offset = horizontal ? new int2(1,0) : new int2(0,1);
            for (int i = 0; i < length; i++)
            {
                var oc = startPosition + offset * i;
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

        protected override bool Mutate()
        {
            bool mutationOccured = false;
            float rShift = UnityEngine.Random.Range(0f, 1f);
            if(rShift > MazeBuilderMetrics.TILE_SHIFT_PROBABILITY)
                return false;
            int rX = UnityEngine.Random.Range(x - 1, x + 2); //max exclusive
            int rY = UnityEngine.Random.Range(y - 1, y + 2);
            if(rX >= 0 && rX < MazeBuilderMetrics.WIDTH && rX != x)
            {
                x = rX;
                mutationOccured = true;
            }
            if(rY >= 0 && rY < MazeBuilderMetrics.HEIGHT && rY != y)
            {                
                y = rY;		
                mutationOccured = true;
            }

            return mutationOccured;
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