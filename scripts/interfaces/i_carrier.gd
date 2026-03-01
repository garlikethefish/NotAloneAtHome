extends Helper

class_name ICarrier

signal on_carry_start(carriable: ICarriable)
signal on_carry_stop(carriable: ICarriable)

var can_carry_callable: Callable

var carriable: ICarriable
var is_carrying: bool:
	get: return carriable != null
	
var facingDirection := Vector2(20,0)

func _process(_delta):
	carry(_delta)
	
func toggle_carry(_cariable: ICarriable):
	if !is_carrying:
		try_carry_start(_cariable)
	else:
		carry_stop()

func carry_stop(show_animation: bool = true):
	if !carriable: return
	
	carriable.drop(show_animation)
	on_carry_stop.emit(carriable)
	carriable = null
	print("Stoped carry")
	
func try_carry_start(_cariable: ICarriable) -> bool:
	if !_cariable or !_cariable.can_be_carried(self) or !can_carry(_cariable): return false
	print("Started carry")
	
	# cancels active pos tween
	_cariable.get_position_tween().kill()
	carriable = _cariable
	carriable.pick_up(self)
	is_carrying = true
	on_carry_start.emit(_cariable)
	return true
	
func carry(delta: float):
	if !is_carrying: return
	
	var carriableParent = carriable.helper_holder.main_parent
	# smooth interpolation
	carriableParent.global_position = carriableParent.global_position.lerp(global_position + facingDirection * 20, 1.0 - pow(1.0 - 0.1, delta * 60))
	
func can_carry(_carriable: ICarriable) -> bool:
	return can_carry_callable.call(_carriable)
	
func assert_assigned_callables() -> void:
	assert_callable(can_carry_callable, "can_carry_callable")
