class_name BlockerQueue

signal on_blocker_add(helper: Helper)
signal on_blocker_remove(helper: Helper)

var blocker_seats: Array[Helper] = []
var is_blocked: bool: 
	get(): return !blocker_seats.is_empty() 


func add_blocker(helper: Helper):
	if !blocker_seats.has(helper):
		blocker_seats.append(helper)
		on_blocker_add.emit(helper)


func remove_blocker(helper: Helper):
	if blocker_seats.has(helper):
		blocker_seats.erase(helper)
		on_blocker_remove.emit(helper)


func print():
	if blocker_seats.is_empty():
		print("Empty")
	else:
		for blocker in blocker_seats:
			print(blocker)
