extends Node2D

class_name ICarrier

@export var hoverOffset := Vector2.ZERO
var iCariable: ICariable
var isCarrying: bool:
	get: return iCariable != null

func _process(_delta):
	carry()
	
func try_to_carry(_cariable: ICariable):
	if !isCarrying and _cariable and _cariable.can_be_carried(self):
		carry_start(_cariable)
	else:
		carry_stop()

func carry_stop():
	if !iCariable: return
	
	iCariable.drop()
	iCariable = null
	print("Stoped carry")
	
func carry_start(_cariable: ICariable):
	if iCariable == _cariable:
		carry_stop()
		return
	
	if _cariable and _cariable.can_be_carried(self) and can_carry(_cariable):
		_cariable.pick_up(self)
		iCariable = _cariable
		iCariable.pick_up(self)
		print("Started carry")
	
func carry():
	if !isCarrying: return
	
	iCariable.get_parent().global_position = global_position + hoverOffset
	
func can_carry(_cariable: ICariable) -> bool:
	var parent := get_parent()
	return parent != null and parent.has_method("can_carry") and parent.can_carry(_cariable)
