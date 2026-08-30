using System;
using System.Collections.Generic;
using System.IO;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public sealed class GlobalSnoData {
	public GlobalSnoData(Stream? stream, bool hasPayload) {
		using var _ = new PerfCounter("GlobalSnoData::cctor`Stream`bool");
		if (stream == null) {
			return;
		}

		var magic = stream.Read<uint>();
		if (magic != 0x44CF00F5) {
			throw new InvalidDataException("Not an global SNO meta file");
		}

		var count = stream.Read<int>();
		Buffers.EnsureCapacity(count);

		var offset = stream.Position + count * 8;
		var payloadPtrSize = hasPayload ? 8 : 0;

		for (var index = 0; index < count; ++index) {
			var next = stream.Position + 8;
			var id = stream.Read<uint>();
			var size = stream.Read<int>();
			stream.Position = offset + payloadPtrSize;
			Buffers[id] = stream.ReadArray<byte>(size);
			offset += Align(size, 8) + payloadPtrSize;
			stream.Position = next;
		}
	}

	private static int Align(int value, int n) => unchecked(value + (n - 1)) & ~(n - 1);

	// Normally I would use rented arrays here but that would require me to rewrite a good chunk of TACT to respect IDisposable
	public Dictionary<uint, ReadOnlyMemory<byte>> Buffers { get; set; } = [];
}
