# Tic Tac Toe

Command line version of the classic Tic Tac Toe game.

## Tutorial

### Customize

Define board size by specifying command line arguments. First argument stands for the number of rows and second argument for the number of columns. In case you provide an invalid number of parameters or the given numbers are not positive numbers an exception is thrown.

You can also define your own number of fields that are needed to match to win the game. The default value is 5. If you wish to change this value, add your preferred positive number as the third CL argument. The input value is checked against the game board dimensions and in case of an invalid value an exception is thrown.

### Play

For each move, write the desired symbol coordinates to the console with respect to the following template:

`row_index` `column_index`

Both rows and columns are indexed from 0. If you provide coordinates that are not within the game board, the error message is displayed and you are prompted to give new coordinates.

Once the number of matched fields reaches the `fields_to_win` value, `GAME OVER` message is shown.
