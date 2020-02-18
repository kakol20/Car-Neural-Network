function drawBetweenPoints(vector1, vector2, r, g, b, weight)
{
	strokeWeight(weight);
	stroke(r, g, b);

	line(vector1.x, vector1.y, vector2.x, vector2.y);
}

function calculatePosition(generation, maxGeneration, fitness, maxFitness)
{
	let x = (generation + 1) / (maxGeneration + 2);
	x *= width;

	let y = (fitness + 1) / (maxFitness + 2);
	y *= height;
	y = height - y;

	//console.log({x: x, y: y});	

	return createVector(x, y);
}

let jsonData = {};

function preload()
{
	// const url = 'trainingLog.json';

	jsonData = loadJSON('trainingLog.json');
}

function setup()
{
	noLoop();

	//console.log(jsonData);

	createCanvas(1280, 720);
}

function draw()
{
	let data = jsonData.Items;

	noFill();

	data.sort(function(a, b)
	{
		return a.generation - b.generation;
	});

	console.log(data);

	let maxFitness = 0;

	for (let i = 0; i < data.length; i++)
	{
		if (data[i].medianFitness > maxFitness) maxFitness = data[i].medianFitness;

		if (data[i].topFitness > maxFitness) maxFitness = data[i].topFitness;
	}

	strokeWeight(4);
	for (let i = 0; i < data.length; i++)
	{
		stroke(255, 64, 64);
		let prev = createVector(0, 0);
		if (i == 0)
		{
			prev = calculatePosition(0, data.length, 0, maxFitness);
		}
		else
		{
			prev = calculatePosition(data[i - 1].generation, data.length, data[i - 1].topFitness, maxFitness);
		}

		let curr = calculatePosition(data[i].generation, data.length, data[i].topFitness, maxFitness);

		line(prev.x, prev.y, curr.x, curr.y);

		stroke(64, 64, 255);
		if (i == 0)
		{
			prev = calculatePosition(0, data.length, 0, maxFitness);
		}
		else
		{
			prev = calculatePosition(data[i - 1].generation, data.length, data[i - 1].medianFitness, maxFitness);
		}

		curr = calculatePosition(data[i].generation, data.length, data[i].medianFitness, maxFitness);

		line(prev.x, prev.y, curr.x, curr.y);
	}
	
}
