# antrunner
An experiment with a mix of compute shader and vertex shader to render as many low poly ants as possible (even with animation!)  
The performance is very impressive with hundreds of thousand of ants that skitter pseudo randomly around in their little vertical slice of the box.  
The ants are confined to a specific horizontal space, so they are all always in view of the orthographic camera and are always rendered.  
Still ~15 FPS with nearly 900k ants on a NVidia GeForce RTX 2080 Ti:  
![ants_build](./readme_pics/Screenshot%202024-07-11%20120911.png)  
View from the side with the scene camera:  
![ants_scene](./readme_pics/Screenshot%202024-07-11%20121025.png)