using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
* Pipeline Structure:
*
* Nessecary Input: Chunk Library
* Optional Input: Graph
* 
*
*
**/
public class PLGPipeline
{

    public List<PLGStage> stages;

    public void Generate()
    {
        var stage1 = new GraphGenerationPLGState<int>();
        var stage2 = new GraphRasterizationStage<int>();

        var output = stage1.Generate(new GraphParameters());
        var final = stage2.Generate(output);

    }
}

public class GraphParameters
{

}



public class RasterizedGraph
{

}

public abstract class PLGStage
{
    public abstract System.Type GetInputType();
    public abstract System.Type GetOutputType();
}

public abstract class PLGStage<In,Out> : PLGStage
{
    In input;
    Out output;
    public abstract bool ValidateInput(In input);
    public abstract bool ValidateOutput(Out output);
    public abstract Out Generate(In input);

    public override Type GetInputType()
    {
        return input.GetType();
    }

    public override Type GetOutputType()
    {
        return output.GetType();
    }
}

public class GraphGenerationPLGState<T> : PLGStage<GraphParameters, Graph<T>>
{
    public override bool ValidateInput(GraphParameters input)
    {
        return false;
    }

    public override bool ValidateOutput(Graph<T> output)
    { 
        return false;
    }

    public override Graph<T> Generate(GraphParameters input)
    {
        return null;
    }
}

public class GraphRasterizationStage<T> : PLGStage<Graph<T>, RasterizedGraph>
{
    public override bool ValidateInput(Graph<T> input)
    {
        return false;
    }

    public override bool ValidateOutput(RasterizedGraph output)
    { 
        return false;
    }

    public override RasterizedGraph Generate(Graph<T> input)
    {
        return null;
    }
}

public class RoomLayoutPLGPhase<T> : PLGStage<Graph<T>, RasterizedGraph>
{
    public override bool ValidateInput(Graph<T> input)
    {
        return false;
    }

    public override bool ValidateOutput(RasterizedGraph output)
    { 
        return false;
    }

    public override RasterizedGraph Generate(Graph<T> input)
    {
        return null;
    }
}