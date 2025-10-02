using Godot;
using System;
using BoilerTronicsObjects.Layers;

namespace BoilerTronicsObjects.Placeable
{
    public abstract partial class PlaceableObject
    {
        // Values used to keep track of the position of the object and what sprite it is
        int OGX { get; set; }
        int OGY { get; set; }
        int CurrX => OGX;
        int CurrY => OGY;
        int sourceId { get; init; }               // This is the id of the tile map that the sprite belongs to
        Vector2I atlasPos { get; init; }          // Posistion on the atlas that the sprite is at
        int altTitle { get; init; }               // This will allow us to set the sprite to alternative sprites

        public Vector2I GetOGPos()
        {
            return new Vector2I(OGX, OGY);
        }
        
        public Vector2I GetCurrPos()
        {
            return new Vector2I(CurrX, CurrY);
        }

        // move the OG posistion of the object
        public void MoveObject(int newX, int newY)
        {
            OGX = newX;
            OGY = newY;
        }

        public Vector2I GetPos()
        {
            return new Vector2I(CurrX, CurrY);
        }

        public int GetSourceID()
        {
            return sourceId;
        }
        
        public Vector2I GetAtlasPos()
        {
            return atlasPos;
        }
    }
}
