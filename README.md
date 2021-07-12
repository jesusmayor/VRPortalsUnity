# Partially non Euclidean Maze Procedural Generator VR

Procedural maze generator that creates random enviroments based on the user´s available workspace using non euclidean geometry.

When people use virtual reality devices, depending on the level of presence on the virtual enviroment they feel, they often forget about the limitations of the available space on the real world. The objetive of this project is to solve this problem when the user moves using Room Scale movement in virtual reality, to avoid hitting walls or other objects in the real world while exploring the virtual enviroment.

To accomplish this, partially non euclidean enviroments are used. The maze is generated in different sections, that are connected by portals that render the view that the user would see if these sections were contiguous. The maze can be explored since the user can walk across these portals and get teleported to the section connected.

The mazes generated are stored in a data structure that works as a graph but provides tool to manipulate them as needed.

(Foto de Portales)

The sections generated have L,T or F forms, where T and F sections generate new ramifications since they have several exits. 

(Foto de seccion y visualización de los portales)

The maze is generated dinamically as the user explores the maze, rendering only the sections needed at each moment to improve performance. They are rendered in different points of the scene to avoid section overlapping.

(Foto del laberinto desde arriba)

## How to set

The procedural generator takes in some information and parameters to create the enviroment:

(Foto de las opciones)

* Maze Length: Number of sections to generate. These sections form the maze as a whole.
* Max Ramification Length: Maximum number of sections of the ramifications created by T and F form sections.
* WorkSpace: Dimensions of the workspace available by the user.
* Player Start Point: Coordinates of the start point within the workspace.
