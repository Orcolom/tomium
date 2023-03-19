import "Unity" for GameObject, Transform, WrenComponent

var go = GameObject.New("on construct")
go.name = "named from wren"

var transform1 = go.GetComponent(Transform)
var pos = transform1.GetPosition()
pos.x = 10
transform1.SetPosition(pos)

var X = Fn.new {
  // wont work
  transform1.GetPosition().x = transform1.GetPosition().x + 0.1
}


class Runner is WrenComponent {

  construct New(){}

  Awake() {
    System.print("invoke awake")
    
    _transform = this.gameObject.GetComponent(Transform)
  }

  Start() {
    System.print("invoke start")
  }

  Update() {
    var position = _transform.GetPosition()
    position.x  = position.x + 1
    _transform.SetPosition(position)
    _transform.gameObject.name = "from wren %(position)"
  }

  WontWork(transform) {
    // wont work, common c# struct and properties issue
    transform.position.x = transform.position.x + 1
  }
}

go.AddComponent(Runner)