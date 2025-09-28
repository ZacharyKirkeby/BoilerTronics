using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Placable;

namespace BoilerTronicsObjects.Layers
{
    public partial class Layer : Godot.TileMapLayer
    {
        ArrayList objectList = new ArrayList();

        public void addObject(PlacableObject newPlacabel)
        {
            objectList.Add(newPlacabel); // adds the placabel to the list of objects on this layer
        }

    }
}