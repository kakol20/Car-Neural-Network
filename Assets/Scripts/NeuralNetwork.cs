using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NeuralNetwork
{
    private readonly int[] layers;
    private float[][] biases;
    private float mutationRate = 0.1f;
    private float[][] neurons;
    private float[][][] weights;

    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        InitNeurons();
        InitBiases();
        InitWeights();

        //Debug.Log(10 / mutationRate);
    }

    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        this.layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < copyNetwork.layers.Length; i++)
        {
            this.layers[i] = copyNetwork.layers[i];
        }

        InitNeurons();

        InitBiases();

        CopyBiases(copyNetwork.biases);

        InitWeights();

        CopyWeights(copyNetwork.weights);
    }

    public void CopyBiases(float[][] copyBiases)
    {
        for (int i = 0; i < copyBiases.Length; i++)
        {
            for (int j = 0; j < copyBiases[i].Length; j++)
            {
                biases[i][j] = copyBiases[i][j];
            }
        }
    }

    public void CopyWeights(float[][][] copyWeights)
    {
        for (int i = 0; i < copyWeights.Length; i++)
        {
            for (int j = 0; j < copyWeights[i].Length; j++)
            {
                for (int k = 0; k < copyWeights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    public float[] FeedForward(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;

                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                value += biases[i][j];

                if (i == layers.Length - 1)
                {
                    value = (2 / (1 + Mathf.Exp(-1 * value))) - 1f; // sigmoid function between -1 and 1
                }
                else
                {
                    value = (float)Math.Tanh(value); // tanh function between -1 and 1;
                }

                neurons[i][j] = value;
            }
        }

        return neurons[neurons.Length - 1];
    }

    public float[][] GetBiases()
    {
        return biases;
    }

    public float GetMutationRate()
    {
        return mutationRate;
    }

    public float[][][] GetWeights()
    {
        return weights;
    }

    public void Mutate()
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    // mutate weight - basic mutation for now

                    //weight += Random.Range(-1f, 1f);
                    //weight = Mathf.Clamp(weight, -1.0f, 1.0f);

                    //weights[i][j][k] = weight;

                    float rand = Random.Range(0f, 10f / mutationRate);

                    if (rand <= 2f) // flip sign
                    {
                        weight *= -1f;
                    }
                    else if (rand <= 4f) // pick random weight between -1 and 1
                    {
                        weight = Random.Range(-1f, 1f);
                    }
                    else if (rand <= 6f) // randomly increase by 0% to 100%
                    {
                        weight *= Random.Range(1f, 2f);
                    }
                    else if (rand <= 8f) // random decress by 0% to 100%
                    {
                        weight *= Random.Range(0f, 1f);
                    }
                    else if (rand <= 10f)
                    {
                        weight += Random.Range(-0.5f, 0.5f);
                    }

                    weights[i][j][k] = Mathf.Clamp(weight, -1f, 1f);
                }
            }
        }

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                float bias = biases[i][j];

                //bias += Random.Range(-1f, 1f);
                //bias = Mathf.Clamp(bias, -1.0f, 1.0f);

                //biases[i][j] = bias;

                float rand = Random.Range(0f, 10f / mutationRate);

                if (rand <= 2f) // flip sign
                {
                    bias *= -1f;
                }
                else if (rand <= 4f) // pick random weight between -1 and 1
                {
                    bias = Random.Range(-1f, 1f);
                }
                else if (rand <= 6f) // randomly increase by 0% to 100%
                {
                    bias *= Random.Range(1f, 2f);
                }
                else if (rand <= 8f) // random decress by 0% to 100%
                {
                    bias *= Random.Range(0f, 1f);
                }
                else if (rand <= 10f)
                {
                    bias += Random.Range(-0.5f, 0.5f);
                }

                biases[i][j] = Mathf.Clamp(bias, -1f, 1f);
            }
        }
    }

    public void Reproduce(NeuralNetwork parent1)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float choose = Random.Range(0.0f, 10.0f);

                    weights[i][j][k] = choose <= 5.0f ? parent1.GetWeights()[i][j][k] : weights[i][j][k];
                }
            }
        }

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                float choose = Random.Range(0.0f, 10.0f);

                biases[i][j] = choose <= 5.0f ? parent1.GetBiases()[i][j] : biases[i][j];
            }
        }

        Mutate();
    }

    public void SetMutationRate(float mutationRate)
    {
        this.mutationRate = mutationRate;
    }

    private void InitBiases()
    {
        List<float[]> biasesList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            List<float> bias = new List<float>();

            for (int j = 0; j < layers[i]; j++)
            {
                bias.Add(Random.Range(-1.0f, 1.0f));
            }

            biasesList.Add(bias.ToArray());
        }

        biases = biasesList.ToArray();
    }

    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }

        neurons = neuronsList.ToArray();
    }

    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();

        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightList = new List<float[]>();

            int neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];

                // set weights randomly
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    // give random weights to neuron weights

                    neuronWeights[k] = Random.Range(-1.0f, 1.0f);
                }

                layerWeightList.Add(neuronWeights);
            }

            weightsList.Add(layerWeightList.ToArray());
        }

        weights = weightsList.ToArray();
    }
}