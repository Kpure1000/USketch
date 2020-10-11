# U Sketch

__A Sketcher of Bezier/B-Spline Curves by Unity3D__



## TODO



- [x] **INSERT control point**
- [x] __DRAG control point__
- [x] __SHOW control point position__
- [x] __SHOW feature polygon__
- [x] __SHOW Convex Hull polygon__
- [ ] __SHOW Knot position of B-Spline__
- [ ] __SHOW controlled segment on curve of B-spline control points__
- [x] __UP/DOWN degree of B-spline__
- [ ] __UP/DOWN degree of Bezier__
- [ ] __SCALE and DRAG Camera View__
- [ ] **SELECT and DRAG control points in a painted rectangle range**
  - [ ] **DRAW a rectangle range**
  - [ ] **CREATE new State in point manager**
  - [ ] 



## Some new design

* Point Manager 里面更新控制点的部分用状态机重新写一下，不然很难添加功能
* 关于实现框选拖动控制点，可以在现有基础上直接框选，按下右键再释放，比较始末位置框选出控制点，加入到拖动列表内，最后只需要在原有拖动功能时判断点是否在列表内，在的话连列表一起拖动，这不影响原有的拖动功能
  * 按下鼠标右键，作为初始点，移动鼠标，同步画框
  * 释放鼠标右键，作为结束点，此时以始末点生成一个RECT，将在范围内的控制点加入拖动列表
  * 拖动点时，判断该点是否在列表内
    * 在列表内，连所有列表内点一起拖动
    * 不在列表内，仅拖动自己
