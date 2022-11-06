import "Unity" for GameObject, Transform, WrenComponent

var go = GameObject.New("s")
go.Name = "from wren"

var transform1 = go.GetComponent(Transform)
var pos = transform1.position
pos.X = 10
transform1.position = pos

var X = Fn.new {
  transform1.position.X = transform1.position.X + 0.1
}


class Runner is WrenComponent {

  start() {
    System.print("start")
  }
  
}

go.AddComponent(Runner)