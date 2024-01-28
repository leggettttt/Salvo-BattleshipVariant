
//place ships on the grid, shiplocations
using System;
using System.Net.Http.Headers;

internal class Program
{


    private static void Main(string[] args)
    {

        //initialise game variables
        List<int> RemainingShips = new List<int> { 5, 4, 3, 2 };

        int[,] Board = {{0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0} };

        int[,] ShipLocations = {{0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0} };

        //place ships
        foreach (int shipID in RemainingShips)
        {
            placeShip(ref ShipLocations, shipID);
        }
        //init shot counter
        int shotCount = 0;
        int salvoSize = 4;
        //play game!!
        while (RemainingShips.Count > 0)
        {
            List<int[]> salvo = getNextSalvo(Board, RemainingShips, salvoSize);

            foreach (int[] shot in salvo)
            {
                updateGame(shot, ref Board, ref ShipLocations, ref RemainingShips);
            }

            displayBoard(Board);
            salvoSize = Math.Max(salvoSize - 1, 1);  // Decrease salvo size by 1, but keep a minimum of 1
            Console.ReadLine();
            shotCount++;
        }
        double accuracy = ((100.0 / (float)shotCount) * 14);
        


        Console.WriteLine("Congratulation, you have sunk all of the ships in {0} shots", shotCount);
        Console.WriteLine("Accuracy : " + accuracy);
        //displayBoard(ShipLocations);
        Console.ReadLine();


    }

   
    static void placeShip(ref int[,] ShipLocations, int shipID)
    {
        bool valid = false;
        Random r = new Random();
        //store coords for ship
        List<int[]> ship = new List<int[]> { };

        while (!valid)
        {
            ship.Clear();
            //generate direction only one (hIncrement or vIncrement) can be 1
            int hIncrement = 0 + r.Next(2);
            int vIncrement = 1 - hIncrement;
            //generate position of head
            ship.Add(new int[] { r.Next(10), r.Next(10) });
            //generate position of rest of ship
            for (int i = 1; i < shipID; i++)
                ship.Add(new int[] { ship[i - 1][0] + vIncrement, ship[i - 1][1] + hIncrement });
            //test each square the ship will occupy
            foreach (int[] i in ship)
            {
                if (i[0] > 9 || i[1] > 9)
                {
                    valid = false;
                    break;
                }
                else if (ShipLocations[i[0], i[1]] != 0)
                {
                    valid = false;
                    break;
                }
                else
                {
                    valid = true;
                }

            }

        }
        //valid location, place ship
        foreach (int[] i in ship)
            ShipLocations[i[0], i[1]] = shipID;

    }

    static void updateGame(int[] NextShot, ref int[,] Board, ref int[,] ShipLocations, ref List<int> RemainingShips)
    {
        //check for hit/miss/sunk
        if (ShipLocations[NextShot[1], NextShot[0]] == 0)
        {
            Board[NextShot[1], NextShot[0]] = -1;      //miss  
        }
        else  //if there is a hit, find out which ship has been hit - remember shot is x,y but board is y,x
        {
            int shipID = ShipLocations[NextShot[1], NextShot[0]]; //get the ID of the ship that has been hit
            ShipLocations[NextShot[1], NextShot[0]] = shipID * -1; //update ship location grid, hit square is no longer 'has a ship in it' so invert shipID

            if (isSunk(ref ShipLocations, shipID)) //check if shipID is still active on board
            {
                RemainingShips.Remove(shipID); //update remaining ships
                //set hits to sunk. search shiplocations for inverse shipID
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (ShipLocations[i, j] == shipID * -1)
                            Board[i, j] = 2;   //hit and sunk
                    }
                }
            }
            else
            {
                Board[NextShot[1], NextShot[0]] = 1;   //hit not sunk
            }
        }
    }

    //check shiplocation board for any occurance of specified ship value {5,4,3,2]
    static bool isSunk(ref int[,] ShipLocations, int shipID)
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (ShipLocations[i, j] == shipID)
                    return false;  //ship has at least one square remaining
            }
        }
        //ship is sunk!
        return true;

    }


    private static List<int[]> getNextSalvo(int[,] Board, List<int> RemainingShips, int Shots)
    {
        List<int[]> salvo = new List<int[]>();

        // Determine the number of shots to take based on the remaining ships
        int remainingShipsCount = RemainingShips.Count;
        int shotsToTake = Math.Min(Shots, remainingShipsCount);

        // Take shots for each remaining ship
        for (int i = 0; i < shotsToTake; i++)
        {
            int[] shot = getNextShot(Board, RemainingShips);
            salvo.Add(shot);
        }

        return salvo;
    }
    private static int[] convertSalvoToSingleShot(int[,] salvo)
    {
        for (int i = 0; i < salvo.GetLength(0); i++)
        {
            for (int j = 0; j < salvo.GetLength(1); j++)
            {
                if (salvo[i, j] == 1)
                {
                    return new int[] { j, i };  // Return the first hit in the salvo
                }
            }
        }
        return new int[] { -1, -1 };  // Invalid shot if no hits found in the salvo
    }
    static int[] getNextShot(int[,] Board, List<int> RemainingShips)
    {
        //cechk how many ships are remaining
        if (RemainingShips.Count == 0)
        {
            //if all ships are sunk, return an invalid shot
            return new int[] { -1, -1 };
        }

        //if htis are found, shoot around hit
        if (hitChecker(Board))
        {
            return getShotAroundHit(Board);
        }

        //if no ships around hit shoot random again until ship is hit 
        return getRandomShot(Board);
    }

    static bool hitChecker(int[,] Board)
    {
        //check if there are any hits on the board
        for (int i = 0; i < Board.GetLength(0); i++)
        {
            for (int j = 0; j < Board.GetLength(1); j++)
            {
                if (Board[i, j] == 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    static int[] getRandomShot(int[,] Board)
    {
        List<int[]> availableShots = new List<int[]>();

        //loop through rows
        for (int i = 0; i < Board.GetLength(0); i++)
        {
            //loop through columns
            for (int j = 0; j < Board.GetLength(1); j++)
            {
                //check if square has been shot at
                if (Board[i, j] == 0)
                {
                    availableShots.Add(new int[] { j, i });
                }
            }
        }
        Random random = new Random();
        //if there are available shots, randomly select one
        if (availableShots.Count > 0)
        {
            return availableShots[random.Next(availableShots.Count)];
        }

        //if no available shots are found, return  invalid shot
        return new int[] { -1, -1 };
    }

    static int[] getShotAroundHit(int[,] Board)
    {
        //loop through rows
        for (int i = 0; i < Board.GetLength(0); i++)
        {
            //loop through columns
            for (int j = 0; j < Board.GetLength(1); j++)
            {
                //chckes if its a hit
                if (Board[i, j] == 1)
                {
                    //chceks around the hit to see if it can shoot 
                    int[] shot = tryGetValidShot(Board, i, j);
                    if (shot != null)
                    {
                        return shot;
                    }
                }
            }
        }

        //if no ships found yet keep shooting random
        return getRandomShot(Board);
    }

    static int[] tryGetValidShot(int[,] Board, int row, int collum)
    {
        //checks above
        if (row > 0 && Board[row - 1, collum] == 0)
        {
            return new int[] { collum, row - 1 };
        }

        //checks below
        else if (row < Board.GetLength(0) - 1 && Board[row + 1, collum] == 0)
        {
            return new int[] { collum, row + 1 };
        }

        //checks to the left
        else if (collum > 0 && Board[row, collum - 1] == 0)
        {
            return new int[] { collum - 1, row };
        }

        //checks to rght
        else if (collum < Board.GetLength(1) - 1 && Board[row, collum + 1] == 0)
        {
            return new int[] { collum + 1, row };
        }

        //no shots found
        return null;
    }


    static void displayBoard(int[,] Board)
    {


        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                switch (Board[i, j])
                {
                    case 0:
                        Console.Write(" 0 ", Console.ForegroundColor = ConsoleColor.Gray); // Not shot at
                        break;
                    case -1:
                        Console.Write("-1 ", Console.ForegroundColor = ConsoleColor.Red); // Miss
                        break;
                    case 1:
                        Console.Write(" 1 ", Console.ForegroundColor = ConsoleColor.Green); // Hit, not sunk
                        break;
                    case 2:
                        Console.Write(" 2 ", Console.ForegroundColor = ConsoleColor.Blue); // Hit and sunk
                        break;
                }

            }
            Console.WriteLine();
        }
        Console.WriteLine();

    }

    static void displayShipLocations(int[,] shipLocations)
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Console.WriteLine(shipLocations[i, j]);
            }
        }
    }

  
}