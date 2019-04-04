# VoxelWorld
Training Unity on a Voxel World

I have created this project to train different aspects of Unity.
I am familiar with Unity itself, most of its features and its workflows.

My first goal was learning about terrain generation, it lead to some problems which I solved mostly. 
I decided for a minecraft "clone" style because that way I have a clear path of features and I can fully focus on the programming part.

This project is not meant for release. It's sole purpose is learning. 

## Goals
 - Terrain Generation: Done, reworking code
 - Player movement: Done
 - Player interactions: Partly done
 - Inventory: Not yet started
 - Voxel interaction in lifetime: Not yet started
 - ECS: Halted until better documentation
 - Character setup: Not yet started
 
I set new goals once I reach already listed goals and when I discover interesting topics.
 
## Problems

I ran into mutliple problems on the way to reaching my goals.

### ECS

Documentation nearly does not exist, presentations on the website / youtube are outdated, not working anymore.
While the github example project shows the use very well it does not explain it, as I said functions are not really documented,
so I can't just look them up. Also it adds layers and layers of complexity that I am not ready for yet. 
Missing Editor interfaces, multiple Editor bugs.

**Result:** Even though I got prototypes to work I decided to wait to look at ECS until it's not in preview state anymore as its current state
rather slows me down than teaches me anything new.

### Performance

Having 1000 voxels on my screen wasn't a problem. Having 5000+ was a problem. I did not expect my high end PC to be overwhelmed that fast.
A bit of research led me to mesh combining.

**Result:** Combining meshes greatly improves the performance. It worked great until...

### Mesh Combining with materials

Now I just had a single material left on all of my voxel faces. While earth or sand would have looked great, grass that has different 
materials for different faces looked... weird. After some hours more experimenting and research I had the current MeshCombiner class
that combined one mesh for each material and then combined everything into one mesh whilst in that last process keeping material indexes.
Attaching the material list and everything was running.
Adding the now missing collider was not a problem as I could just add a MeshCollider to the newly generated mesh

**Result:** Finally mesh combining 

### Performance II

Now that I could have huge terrain without big FPS loss I decided to generate more area... Resulted frozen screen for about 1-2 secs
before all chunks are generated. I got suggested to use ECS which as described above was off limits for me at this time.
I figured out to delay the generation over multiple frames with a Coroutine and waiting after each column with 
`yield return new WaitForEndOfFrame();`

**Result:** This is not a perfect solution and FPS still drop whilst generating chunks but the impact is justifyable at this point.

### Code cluttering

After hours of experimenting I had a hard time to figure out my own code. I started reworking classes and finally rewrote the 
entire VoxelChunk class from scratch with my new knowledge. Other classes will follow.

**Result:** Having 1-2 days of cleaning code between feature sessions will lead to following positive points:
 - Better overview of how the code interacts
 - On the fly improvements or overall improvements when rewriting
 
This point is still TBD

## In a nutshell

**04.04.2019:**
Do not clamp onto one topic and do that one no matter what. Let problems rest for a day or two if you get stuck. 
ECS almost cost me my motivation to go forward until I realised I had dug to deep. ECS is unfinished and even experienced people 
have their problems. Going forward I will focus on more essential parts.

Remember: You learn best when you have fun at learning.
