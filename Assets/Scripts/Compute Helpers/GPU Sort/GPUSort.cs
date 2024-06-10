using UnityEngine;
using static UnityEngine.Mathf;

public class GPUSort
{
    const int sortKernel = 0;
    const int calculateOffsetsKernel = 1;

    readonly ComputeShader sortCompute;
    ComputeBuffer indexBuffer;

    //// Tested Object 
    readonly ComputeShader TestedObjectSortCompute;
    ComputeBuffer TestedObjectindexBuffer;


    public GPUSort()
    {
        sortCompute = ComputeHelper.LoadComputeShader("BitonicMergeSort");

        TestedObjectSortCompute = ComputeHelper.LoadComputeShader("BitonicMergeSortTesetedObject");
    }

    public void SetBuffers(ComputeBuffer indexBuffer, ComputeBuffer offsetBuffer)
    {
        this.indexBuffer = indexBuffer;

        sortCompute.SetBuffer(sortKernel, "Entries", indexBuffer);
        ComputeHelper.SetBuffer(sortCompute, offsetBuffer, "Offsets", calculateOffsetsKernel);
        ComputeHelper.SetBuffer(sortCompute, indexBuffer, "Entries", calculateOffsetsKernel);
    }

    // Tested Object 
    public void SetPointsBuffers(ComputeBuffer indexBuffer, ComputeBuffer offsetBuffer)
    {
        this.TestedObjectindexBuffer = indexBuffer;

        TestedObjectSortCompute.SetBuffer(sortKernel, "Entries", indexBuffer);
        ComputeHelper.SetBuffer(TestedObjectSortCompute, offsetBuffer, "Offsets", calculateOffsetsKernel);
        ComputeHelper.SetBuffer(TestedObjectSortCompute, indexBuffer, "Entries", calculateOffsetsKernel);
    }

    // Sorts given buffer of integer values using bitonic merge sort
    // Note: buffer size is not restricted to powers of 2 in this implementation
    public void Sort()
    {
        sortCompute.SetInt("numEntries", indexBuffer.count);

        // Launch each step of the sorting algorithm (once the previous step is complete)
        // Number of steps = [log2(n) * (log2(n) + 1)] / 2
        // where n = nearest power of 2 that is greater or equal to the number of inputs
        int numStages = (int)Log(NextPowerOfTwo(indexBuffer.count), 2);

        for (int stageIndex = 0; stageIndex < numStages; stageIndex++)
        {
            for (int stepIndex = 0; stepIndex < stageIndex + 1; stepIndex++)
            {
                // Calculate some pattern stuff
                int groupWidth = 1 << (stageIndex - stepIndex);
                int groupHeight = 2 * groupWidth - 1;
                sortCompute.SetInt("groupWidth", groupWidth);
                sortCompute.SetInt("groupHeight", groupHeight);
                sortCompute.SetInt("stepIndex", stepIndex);
                // Run the sorting step on the GPU
                ComputeHelper.Dispatch(sortCompute, NextPowerOfTwo(indexBuffer.count) / 2);
            }
        }
    }

    // Tested Object 
    public void SortPoints()
    {
        TestedObjectSortCompute.SetInt("numEntries", TestedObjectindexBuffer.count);

        // Launch each step of the sorting algorithm (once the previous step is complete)
        // Number of steps = [log2(n) * (log2(n) + 1)] / 2
        // where n = nearest power of 2 that is greater or equal to the number of inputs
        int numStages = (int)Log(NextPowerOfTwo(TestedObjectindexBuffer.count), 2);

        for (int stageIndex = 0; stageIndex < numStages; stageIndex++)
        {
            for (int stepIndex = 0; stepIndex < stageIndex + 1; stepIndex++)
            {
                // Calculate some pattern stuff
                int groupWidth = 1 << (stageIndex - stepIndex);
                int groupHeight = 2 * groupWidth - 1;
                TestedObjectSortCompute.SetInt("groupWidth", groupWidth);
                TestedObjectSortCompute.SetInt("groupHeight", groupHeight);
                TestedObjectSortCompute.SetInt("stepIndex", stepIndex);
                // Run the sorting step on the GPU
                ComputeHelper.Dispatch(TestedObjectSortCompute, NextPowerOfTwo(TestedObjectindexBuffer.count) / 2);
            }
        }
    }

    public void SortAndCalculateOffsets()
    {
        Sort();

        // Tested Object 
        SortPoints();

        ComputeHelper.Dispatch(sortCompute, indexBuffer.count, kernelIndex: calculateOffsetsKernel);

        // Tested Object 
        ComputeHelper.Dispatch(TestedObjectSortCompute, TestedObjectindexBuffer.count, kernelIndex: calculateOffsetsKernel);
    }

}