
002ee830 <System_Threading_ThreadLocal_1_get_Value>:
	;; Save old stack base, set up a new stack
  2ee830:	55                   	push   %ebp
  2ee831:	8b ec                	mov    %esp,%ebp
	;; Save EBX
  2ee833:	53                   	push   %ebx
	;; Add 4 bytes to stack.
  2ee834:	83 ec 04             	sub    $0x4,%esp
	;; Use call to get address of this function (essentially).
  2ee837:	e8 00 00 00 00       	call   2ee83c <System_Threading_ThreadLocal_1_get_Value+0xc>
	;; Pop 0x2ee837 EBX. The number added to it
	;; makes me think this is for diagnostics if an exception is
	;; thrown and we need a stack trace.
  2ee83c:	5b                   	pop    %ebx
  2ee83d:	81 c3 74 49 24 00    	add    $0x244974,%ebx
	;; Move 0x2ee837 to EAX.
  2ee843:	8b 45 08             	mov    0x8(%ebp),%eax
	;; Make space for 12 bytes on stack
  2ee846:	83 ec 0c             	sub    $0xc,%esp
	;; Save address 0x2ee837 to stack, for tracing stack later?
  2ee849:	50                   	push   %eax
	;; Alignment
  2ee84a:	90                   	nop
	;; Call local member. May trigger an exception and never return here.
  2ee84b:	e8 e0 02 00 00       	call   2eeb30 <System_Threading_ThreadLocal_1_ThrowIfNeeded>
	;; Reclaim 16 bytes of space from the stack.
  2ee850:	83 c4 10             	add    $0x10,%esp
	;; Save address 0x2ee837 to stack again.
  2ee853:	8b 45 08             	mov    0x8(%ebp),%eax
  2ee856:	83 ec 0c             	sub    $0xc,%esp
  2ee859:	50                   	push   %eax
  2ee85a:	90                   	nop
	;; Call local member. Will leave result in EAX.
  2ee85b:	e8 90 01 00 00       	call   2ee9f0 <System_Threading_ThreadLocal_1_GetValueThreadLocal>
	;; Restore stack before returning.
  2ee860:	83 c4 10             	add    $0x10,%esp
  2ee863:	8d 65 fc             	lea    -0x4(%ebp),%esp
  2ee866:	5b                   	pop    %ebx
  2ee867:	c9                   	leave  
  2ee868:	c3                   	ret    
	...
