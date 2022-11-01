import "Unity" for GameObject, Transform

var go = GameObject.New("s")
// go.Name = "from wren"

var transform1 = go.GetComponent(Transform)
// var transform2 = go.GetComponent("%(Transform)")
// var transform3 = go[Transform]
