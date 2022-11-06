import "Unity" for GameObject, Transform

var go = GameObject.New("s")
go.Name = "from wren"

var transform1 = go.GetComponent(Transform)
var pos = transform1.position
pos.X = 10
transform1.position = pos

var X = Fn.new {
  transform1.position.X = transform1.position.X + 0.1
}