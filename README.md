# U Sketch

__A Sketcher of Bezier/B-Spline Curves by Unity3D__



## Usage

* Click Right on space place without point to **INSERT** control point
* Hold Right on control point and  move to **DRAG** it
* Hold Left on space place without point and move to draw a rectangle to **SELECT** control point(s)
  * The selected control point(s) will be **HIGHLIGHT** and 
    * Can be **DRAG** together
    * Can be **DELETE** together
* Move cursor on right operator UI to select **function**
  * open dropdown list and **select** curve between Bezier and B-spline
  * play/exit lamping **animation**
  * public properties
    * show **position text** of control point(s)
    * show **featural polygon** of control points
    * show **convex hull** of featural polygon
    * up/down degree
  * B-spline properties
    * show **knot point(s)** on curve

* move to right-bottom button and press it to **QUIT** program.

## TODO


- [x] __INSERT control point__
- [x] __REMOVE control point__
- [x] __DRAG control point__
- [x] __SHOW control point position__
- [x] __SHOW feature polygon__
- [x] __SHOW Convex Hull polygon__
- [x] __SHOW Knot position of B-Spline__
- [ ] __SHOW controlled segment on curve of B-spline control points__
- [x] __UP/DOWN degree of B-spline__
- [x] __UP/DOWN degree of Bezier__
- [x] __SCALE and DRAG Camera View__
- [x] **SELECT and DRAG control points in a painted rectangle range**
  - [x] **DRAW a rectangle range**
  - [x] **CREATE new State in point manager** 



## Some new design

* Point Manager 里面更新控制点的部分用状态机重新写一下，不然很难添加功能
