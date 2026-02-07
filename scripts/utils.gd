extends Node
class_name Utils

static func has_child_of_type(parent: Node, type: Variant) -> bool:
	for child in parent.get_children():
		if is_instance_of(child, type):
			return true
	return false
	
static func try_get_child_of_type(parent: Node, type: Variant) -> Variant:
	if parent == null: return null
	for child in parent.get_children():
		if is_instance_of(child, type):
			return child
	return null

static func try_get_parent_of_type(node: Node, type: Variant) -> Variant:
	if node == null: return null
	if is_instance_of(node.get_parent(), type):
		return node.get_parent()
	return null

# Goes up until finds
static func find_parent_of_type(node: Node, script: Script) -> Node:
	while node != null:
		if node.get_script() == script:
			return node
		node = node.get_parent()
	return null
