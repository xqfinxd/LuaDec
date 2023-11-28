local test = {}

function test:foo(a)
	return type(self)
end

print(test:foo(1,"2:"))