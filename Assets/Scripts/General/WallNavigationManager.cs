﻿using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallNavigationManager : MonoBehaviour
{
    public GameStateManager gameState;
    public GameObject fogWallAsset = null;
    private Dictionary<WallAlreadyBuilt, GameObject> setupWalls = new Dictionary<WallAlreadyBuilt, GameObject>();

    void Start()
    {
        if (fogWallAsset == null)
        {
            throw new ArgumentException($"fog wall asset reference must be set in {this.GetType().Name} script.");
        }

        foreach (var tile in gameState.tiles)
        {
            var tileCasted = tile.GetComponent<TileManager>();
            if (tileCasted.westTile != null)
            {
                var name = new WallAlreadyBuilt(tile.name, tileCasted.westTile.name);
                if (!setupWalls.ContainsKey(name))
                {
                    var wallInstance = Instantiate(fogWallAsset, tile.transform.position, tile.transform.rotation);
                    SetupFogWall(wallInstance, name.ToString(), new Vector3(-5f, 0f, 0f), new Vector3(0, 90, 0), tileCasted.westTile, tile, null, null);
                    setupWalls.Add(name, wallInstance);
                }
            }

            if (tileCasted.eastTile != null)
            {
                var name = new WallAlreadyBuilt(tile.name, tileCasted.eastTile.name);
                if (!setupWalls.ContainsKey(name))
                {
                    var wallInstance = Instantiate(fogWallAsset, tile.transform.position, tile.transform.rotation);
                    SetupFogWall(wallInstance, name.ToString(), new Vector3(5f, 0f, 0f), new Vector3(0, 90, 0), tile, tileCasted.eastTile, null, null);
                    setupWalls.Add(name, wallInstance);
                }
            }
            if (tileCasted.northTile != null)
            {
                var name = new WallAlreadyBuilt(tile.name, tileCasted.northTile.name);
                if (!setupWalls.ContainsKey(name))
                {
                    var wallInstance = Instantiate(fogWallAsset, tile.transform.position, tile.transform.rotation);
                    SetupFogWall(wallInstance, name.ToString(), new Vector3(0f, 0f, 5f), new Vector3(0, 0, 0), null, null, tileCasted.northTile, tile);
                    setupWalls.Add(name, wallInstance);
                }
            }
            if (tileCasted.southTile != null)
            {
                var name = new WallAlreadyBuilt(tile.name, tileCasted.southTile.name);
                if (!setupWalls.ContainsKey(name))
                {
                    var wallInstance = Instantiate(fogWallAsset, tile.transform.position, tile.transform.rotation);
                    SetupFogWall(wallInstance, name.ToString(), new Vector3(0f, 0f, -5f), new Vector3(0, 0, 0), null, null, tile, tileCasted.southTile);
                    setupWalls.Add(name, wallInstance);
                }
            }
        }

        EventManager.StartListening(GameObjectEventType.TileCleared, UpdateWall);
    }

    void Update()
    {

    }

    private void SetupFogWall(GameObject fogWall, string name, Vector3 positionOffset, Vector3 rotationOffset, GameObject westTile, GameObject eastTile, GameObject northTile, GameObject southTile)
    {
        fogWall.name = name;
        fogWall.transform.Translate(positionOffset);
        fogWall.transform.Rotate(rotationOffset);
        fogWall.GetComponent<NavigateFog>().tileWest = westTile;
        fogWall.GetComponent<NavigateFog>().tileEast = eastTile;
        fogWall.GetComponent<NavigateFog>().tileNorth = northTile;
        fogWall.GetComponent<NavigateFog>().tileSouth = southTile;
        fogWall.GetComponent<NavigateFog>().Intialize();
    }

    private void UpdateWall(GameObject tile)
    {
        var properties = tile.GetComponent<TileManager>();
        if (properties.isCleared)
        {
            var concernedWalls = setupWalls.Where(p => p.Key.Contains(tile.name));
            foreach (var wall in concernedWalls)
            {
                if (wall.Value.GetComponent<NavigateFog>().BothTileCleared())
                {
                    var child = wall.Value.transform.GetChild(0);
                    child.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            var concernedWalls = setupWalls.Where(p => p.Key.Contains(tile.name));
            foreach (var wall in concernedWalls)
            {
                var child = wall.Value.transform.GetChild(0);
                child.gameObject.SetActive(true);
            }
        }
    }

    private class WallAlreadyBuilt : IEquatable<WallAlreadyBuilt>
    {
        private string _tile1;
        private string _tile2;

        public WallAlreadyBuilt(string tile1, string tile2)
        {
            _tile1 = tile1;
            _tile2 = tile2;
        }

        public bool Equals(WallAlreadyBuilt other)
        {
            return ((other._tile1.Equals(this._tile1) || other._tile2.Equals(this._tile1))
                && (other._tile1.Equals(this._tile2) || other._tile2.Equals(this._tile2)));
        }

        public override bool Equals(object obj)
        {
            return Equals((WallAlreadyBuilt)obj);
        }

        public override int GetHashCode()
        {
            int hash = _tile1.GetHashCode() * 13 + _tile2.GetHashCode() * 13;
            return hash;
        }

        public bool Contains(string name)
        {
            return _tile1.Equals(name) || _tile2.Equals(name);
        }

        public override string ToString()
        {
            return $"{_tile1}-{_tile2}";
        }

    }
}
