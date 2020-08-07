extends Camera	

var camera_speed = 70	
var camera_zoom = 200	

func _physics_process(delta):	
	if Input.is_action_pressed("camera_backwards"):	
		transform.origin += Vector3(0,0,camera_speed*delta)	
	if Input.is_action_pressed("camera_forwards"):	
		transform.origin += Vector3(0,0,-camera_speed*delta)	
	if Input.is_action_pressed("camera_right"):	
		transform.origin += Vector3(camera_speed*delta,0,0)	
	if Input.is_action_pressed("camera_left"):	
		transform.origin += Vector3(-camera_speed*delta,0,0)	
	if Input.is_action_just_released("camera_zoom"):	
		transform.origin += Vector3(0,-camera_zoom*delta,0)	
	if Input.is_action_just_released("camera_amble"):	
		transform.origin += Vector3(0,camera_zoom*delta,0)
