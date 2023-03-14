import "Unity" for GameObject, Transform, WrenComponent

var go = GameObject.New("s")
go.name = "from wren"

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
    System.print("awake")
  }

  Start() {
    System.print("start")
  }

  Update() {
    var transform = this.gameObject.GetComponent(Transform)
    var position = transform.GetPosition()
    position.x  = position.x + 1
    transform.SetPosition(position)
    transform.gameObject.name = "from wren %(position)"
  }

  OptionA(transform) {
    var position = transform.GetPosition()
    position.x = position.x + 1
    transform.SetPosition(position)
  }
  
  OptionB(transform) {
    var position = transform.position
    position.x = position.x + 1
    transform.position = position
  }

  WontWork(transform) {
    // wont work, common c# struct and properties issue
    transform.position.x = transform.position.x + 1
  }

  OptionC(transform) {
    var position = transform.getPosition
    position.x = position.x + 1
    transform.setPosition = position
  }
  
  OptionD(transform) {
    var position = transform.position()
    position.x = position.x + 1
    transform.position(position)
  }
}

go.AddComponent(Runner)