var LoydGame = {

    currTiles: [], // matrix of div elements representing individual tiles
    emptyTile: undefined, // represents empty tile and its value (both undefined)
    tileIndent: 20, // corresponds to the tile size

    inicializeGame: function() {
        let initialTileValues = [
            [3, 11, 2, 5],
            [1, 13, 6, 8],
            [4, 9, undefined, 10],
            [14, 12, 7, 15]
        ];

        for (let i = 0; i < initialTileValues.length; i++) {
            let tileDivRow = [];

            for (let j = 0; j < initialTileValues[i].length; j++) {

                let currValue = initialTileValues[i][j];
                if (currValue !== LoydGame.emptyTile) {
                    let currTile = document.getElementById("tile-" + initialTileValues[i][j]);

                    let currLeftIndent = j * LoydGame.tileIndent;
                    let currTopIndent = i * LoydGame.tileIndent;
                    LoydGame.setTileIndentation(currTile, currLeftIndent, currTopIndent);

                    LoydGame.setClickedTileAction(currTile);
                    tileDivRow.push(currTile);
                } else {
                    tileDivRow.push(LoydGame.emptyTile);
                } 
            }

            LoydGame.currTiles.push(tileDivRow);
        }
    },

    setTileIndentation: function(tile, leftIndent, topIndent) {
        tile.style.left = leftIndent + 'vmin';
        tile.style.top = topIndent + 'vmin';
    },

    setClickedTileAction: function(tile) {
        tile.addEventListener('click', ev => LoydGame.moveTileIfPossible(tile));
    },

    moveTileIfPossible: function(tile) {
        let clickedTileValue = parseInt(tile.textContent);
        let clickedTileIndices = LoydGame.getTileMatrixIndices(clickedTileValue);
        let emptyTileIndices = LoydGame.getTileMatrixIndices(LoydGame.emptyTile);

        let siblingValues = LoydGame.getSiblingValues(emptyTileIndices);
        if (siblingValues.includes(clickedTileValue)) { //clicked tile is empty tile's sibling
            
            // change position style properties (indentLeft, indentTop)
            let indentWithinRow = emptyTileIndices[1] * LoydGame.tileIndent;
            let indentWithinColumn = emptyTileIndices[0] * LoydGame.tileIndent;
            LoydGame.setTileIndentation(tile, indentWithinRow, indentWithinColumn);

            // swap clicked tile and empty tile in LoydGame.currTiles matrix
            LoydGame.currTiles[clickedTileIndices[0]][clickedTileIndices[1]] = LoydGame.emptyTile;
            LoydGame.currTiles[emptyTileIndices[0]][emptyTileIndices[1]] = tile;
            
            if (LoydGame.gameOver()){
                window.alert('Congratulations! You managed to solve the puzzle!');
            }
        } 
    },

    getTileMatrixIndices: function(value) {
        for (let i = 0; i < LoydGame.currTiles.length; i++) {
            for (let j = 0; j < LoydGame.currTiles[i].length; j++) {

                let currTile = LoydGame.currTiles[i][j];
                if (currTile !== LoydGame.emptyTile) {
                    let currTileValue = parseInt(currTile.textContent);
                    if (currTileValue === value) {
                        return [i, j];
                    }
                } else if (currTile === LoydGame.emptyTile && value === LoydGame.emptyTile) {
                    // we are searching for empty tile's indices
                    return [i, j];
                }
            }
        }
        return [-1, -1]; // value not found case (both indices -1);
    },

    getSiblingValues: function(tileIndices){
        let siblingValues = [];
        let siblingDirections = [[0, -1], [0, 1], [-1, 0], [1, 0]]; // horizontal and vertical vectors

        for (let direction of siblingDirections) {
            let siblingRowIndex = tileIndices[0] + direction[0];
            let siblingColumnIndex = tileIndices[1] + direction[1];

            if ((siblingRowIndex < LoydGame.currTiles.length && siblingRowIndex >= 0) // index within currTiles indices range
                && (siblingColumnIndex < LoydGame.currTiles.length && siblingColumnIndex >= 0)) {

                let currSibling = LoydGame.currTiles[siblingRowIndex][siblingColumnIndex];
                let currValue;

                if (currSibling !== LoydGame.emptyTile) {
                    currValue = parseInt(currSibling.textContent);
                } else {
                    currValue = LoydGame.emptyTile;
                }

                siblingValues.push(currValue);
            }
        }
        return siblingValues;
    },

    gameOver: function() {
        let currValue = 0;

        for (let i = 0; i < LoydGame.currTiles.length; i++) {
            for (let j = 0; j < LoydGame.currTiles[i].length; j++) {

                let currTile = LoydGame.currTiles[i][j];
                if ((currTile === LoydGame.emptyTile) 
                    && (i !== LoydGame.currTiles.length - 1 || j !== LoydGame.currTiles[i].length - 1)) {
                        // empty tile position does not match the final tile state (right bottom corner)
                        return false; 
                } else if (currTile !== LoydGame.emptyTile) {

                    let tileValue = parseInt(currTile.textContent);
                    if (tileValue < currValue) {
                        return false; // not sorted correctly
                    }
                    currValue = tileValue;
                }
            }
        }
        let lastRowIndex = LoydGame.currTiles.length - 1;
        let lastColumnIndex = lastRowIndex;

        // all tiles sorted and empty tile placed in the right bottom corner
        return LoydGame.currTiles[lastRowIndex][lastColumnIndex] === LoydGame.emptyTile
    }
};

document.addEventListener('DOMContentLoaded', function(ev){
    let game = Object.create(LoydGame);
    game.inicializeGame();
});
