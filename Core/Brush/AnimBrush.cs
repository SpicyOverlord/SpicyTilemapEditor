using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SpicyTilemapEditor
{

    public class AnimBrush : TilesetBrush
    {
        [Tooltip("The animation frames per second.")]
        public uint AnimFPS = 4;
        [Tooltip("Adds an incremental delay to each brush that is rendered during each camera render.")]
        public float AnimDelay;
        [Tooltip("Delay between animation loops. The number of times, the last frame is repeated before the animation starts over.")]
        public uint LoopFrameDelay;

        [Serializable]
        public class TileAnimFrame
        {
            /// <summary>
            /// Contains the tileData for this frame
            /// </summary>
            public uint tileId; // NOTE: now contains tileData, not just the id
            public Vector2 UVOffset;
            // Idea for animation improvements
            // public float time; //<= 0 means, one per frame, > 0 is the time to stay
            // OR
            // public int frames; //<= 0 means, one per frame, > 0 is the number of frames to stay
        }
        public List<TileAnimFrame> AnimFrames = new();
        // public List<TileAnimFrame> AnimFrames = new List<TileAnimFrame>(new []{new TileAnimFrame(){tileId = Tileset.k_TileData_Empty, UVOffset = Vector2.zero}});

        
        #region IBrush

        public override uint PreviewTileData()
        {
            if (AnimFrames.Count <= 0) 
                return Tileset.k_TileId_Empty;
            
            int animIdx = GetAnimFrameIdx();
            return AnimFrames[animIdx].tileId;
        }

        public override uint Refresh(STETilemap tilemap, int gridX, int gridY, uint tileData)
        {
            if (m_animTileIdx < AnimFrames.Count)
                return (tileData & ~Tileset.k_TileDataMask_TileId) | (uint)AnimFrames[m_animTileIdx].tileId;
            return tileData;
        }

        public override bool IsAnimated() => true;

        private int m_animTileIdx = 0;
        private float m_overrideTime;
        private void UpdateAnimTime(int index = 0)
        {
            float time = Time.realtimeSinceStartup;
            if (AnimDelay != 0f) 
                time += AnimDelay * index;
            m_overrideTime = time;
        }
        private float GetTime() => m_overrideTime != 0f ? m_overrideTime : Time.realtimeSinceStartup;

        public override Rect GetAnimUV( )
        {
            if (AnimFrames.Count == 0) 
                return default(Rect);
            
            int animIdx = GetAnimFrameIdx();            
            TileAnimFrame animFrame = AnimFrames[animIdx];
            uint tileData = animFrame.tileId;
            int tileId = (int)(tileData & Tileset.k_TileDataMask_TileId);
            Rect uv = tileId != Tileset.k_TileId_Empty ? Tileset.Tiles[tileId].uv : default(Rect);
            uv.position += animFrame.UVOffset;                
            return uv;
        }

        public override int GetAnimFrameIdx()
        {
            int animFrames = AnimFrames.Count + (int)LoopFrameDelay;
            return Mathf.Clamp((int)(GetTime() * AnimFPS) % animFrames, 0, AnimFrames.Count - 1);
        }

        public override uint GetAnimTileData()
        {
            if (AnimFrames.Count == 0) 
                return Tileset.k_TileData_Empty;
            
            int animIdx = GetAnimFrameIdx();
            TileAnimFrame animFrame = AnimFrames[animIdx];
            return animFrame.tileId;
        }

        public override Vector2[] GetAnimUVWithFlags(float innerPadding = 0f, int index = 0, uint flags = 0)
        {
            UpdateAnimTime(index);
            Vector2[] ret = base.GetAnimUVWithFlags(innerPadding, index, flags);
            m_overrideTime = 0f; // restore to normal time
            return ret;
        }

        #endregion
    }
}
