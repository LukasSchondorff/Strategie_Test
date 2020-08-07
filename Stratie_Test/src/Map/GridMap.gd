extends GridMap



export var chunk_number:int = 15
var chunk_loader = 32
var width = chunk_number*chunk_loader
const height = 1
var length = width
var open_symplex_new 
var colision_area_factor = 1.5
var pos1:Vector3
var pos2:Vector3
var mutex


func _ready():
	randomize()
	open_symplex_new = OpenSimplexNoise.new()
	open_symplex_new.seed = randi()
	#open_symplex_new.octaves = 4
	#open_symplex_new.period = 256
	#open_symplex_new.lacunarity = 2
	#open_symplex_new.persistence = 0.5
	open_symplex_new.octaves = rand_range(1, 9)
	open_symplex_new.period = rand_range(10, 70)
	open_symplex_new.lacunarity = rand_range(0.1, 4)
	open_symplex_new.persistence = rand_range(0, 1)
	mutex = Mutex.new()
	_generate_world()
	#_generate_collision_area()



func _generate_collision_area():
	get_node("Area/CollisionShape").transform.origin = Vector3(width*colision_area_factor, height*colision_area_factor, length*colision_area_factor)
	get_node("Area/CollisionShape").transform.scaled(Vector3(width*colision_area_factor, height*colision_area_factor, length*colision_area_factor))



func _generate_quadrant(fromto):
	var from = fromto[0]
	var to = fromto[1]
	for x in range(from.x, to.x) :
		for z in range(from.z, to.z) :
			mutex.lock()
			set_cell_item(x, 0, z, _get_tile_index(open_symplex_new.get_noise_3d(float(x), float(0), float(z))))
			mutex.unlock()


func _generate_world():
	var thread = []
	var index = 0
	for chunk in range(chunk_number*chunk_number):
		thread.append( Thread.new())
	for x in range(chunk_number):
		for z in range(chunk_number):
			var from = Vector3(x, 0, z)*chunk_loader
			var to = Vector3(x+1, 0, z+1)*chunk_loader
			var fromto = [from, to]
			thread[index].start(self,"_generate_quadrant", fromto)
			index = index + 1
	for threaddy in thread:
		threaddy.wait_to_finish()



func _get_tile_index(noise_sample):
	if noise_sample < -0.5:
		return 0 #index
	if noise_sample < 0:
		return 1 #index
	if noise_sample < 0.5:
		return 46 #index
	if noise_sample < 1:
		return 27 #index



func _on_Area_input_event(camera, event, click_position, click_normal, shape_idx):
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_LEFT and event.is_pressed() == true:
			print(click_position)
			pos1 = click_position/3
		if event.button_index == BUTTON_LEFT and event.is_pressed() == false:
			print(click_position)
			pos2 = click_position/3
			for x in range(abs(pos2.x-pos1.x)+1):
				for z in range(abs(pos2.z-pos1.z)+1):
					get_cell_item(x + pos1.x,0,z + pos1.z)
					if pos1.x < pos2.x:
						if pos1.z < pos2.z:
							set_cell_item((x+pos1.x), 0, (z+pos1.z) + 1, 32)
						else:
							set_cell_item((x+pos1.x), 0, (z+pos2.z) + 1, 32)
					else:
						if pos1.z < pos2.z:
							set_cell_item((x+pos2.x), 0, (z+pos1.z) + 1, 32)
						else:
							set_cell_item((x+pos2.x), 0, (z+pos2.z) + 1, 32)
		if event.button_index == BUTTON_RIGHT and event.is_pressed() == true:
			set_cell_item((click_position[0]/3), 0, (click_position[2]/3) + 1, 32)















