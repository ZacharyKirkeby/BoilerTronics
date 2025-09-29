using Godot;
using System;
using BoilerTronicsObjects.Layers;

namespace BoilerTronicsObjects.Placable
{
    public abstract partial class PlacableObject
    {
        // Values used to keep track of the position of the object and what sprite it is
        Vector2 OGTilePos;          // Original position of the object
        Vector2 CurrTilePos;        // Current position of the placable object
        int sourceId;               // This is the id of the tile map that the sprite belongs to
        Vector2I atlasPos;          // Posistion on the atlas that the sprite is at
        int altTitle;               // This will allow us to set the sprite to alternative sprites

        public void moveObject(int newX, int newY)
        {
            // changes the original posistion of the object
            OGTilePos.X = newX;
            OGTilePos.Y = newY;
        }

        public Vector2 getPos()
        {
            return CurrTilePos;
        }
    }
}