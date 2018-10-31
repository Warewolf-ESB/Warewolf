"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
const core_1 = require("@angular-devkit/core");
const linked_list_1 = require("./linked-list");
class IndexOutOfBoundException extends core_1.BaseException {
    constructor(index, min, max = Infinity) {
        super(`Index ${index} outside of range [${min}, ${max}].`);
    }
}
exports.IndexOutOfBoundException = IndexOutOfBoundException;
class ContentCannotBeRemovedException extends core_1.BaseException {
    constructor() {
        super(`User tried to remove content that was marked essential.`);
    }
}
exports.ContentCannotBeRemovedException = ContentCannotBeRemovedException;
/**
 * A Chunk description, including left/right content that has been inserted.
 * If _left/_right is null, this means that content was deleted. If the _content is null,
 * it means the content itself was deleted.
 *
 * @see UpdateBuffer
 */
class Chunk {
    constructor(start, end, originalContent) {
        this.start = start;
        this.end = end;
        this.originalContent = originalContent;
        this._left = Buffer.alloc(0);
        this._right = Buffer.alloc(0);
        this._assertLeft = false;
        this._assertRight = false;
        this.next = null;
        this._content = originalContent.slice(start, end);
    }
    get length() {
        return (this._left ? this._left.length : 0)
            + (this._content ? this._content.length : 0)
            + (this._right ? this._right.length : 0);
    }
    toString(encoding = 'utf-8') {
        return (this._left ? this._left.toString(encoding) : '')
            + (this._content ? this._content.toString(encoding) : '')
            + (this._right ? this._right.toString(encoding) : '');
    }
    slice(start) {
        if (start < this.start || start > this.end) {
            throw new IndexOutOfBoundException(start, this.start, this.end);
        }
        // Update _content to the new indices.
        const newChunk = new Chunk(start, this.end, this.originalContent);
        // If this chunk has _content, reslice the original _content. We move the _right so we are not
        // losing any data here. If this chunk has been deleted, the next chunk should also be deleted.
        if (this._content) {
            this._content = this.originalContent.slice(this.start, start);
        }
        else {
            newChunk._content = this._content;
            if (this._right === null) {
                newChunk._left = null;
            }
        }
        this.end = start;
        // Move _right to the new chunk.
        newChunk._right = this._right;
        this._right = this._right && Buffer.alloc(0);
        // Update essentials.
        if (this._assertRight) {
            newChunk._assertRight = true;
            this._assertRight = false;
        }
        // Update the linked list.
        newChunk.next = this.next;
        this.next = newChunk;
        return newChunk;
    }
    append(buffer, essential) {
        if (!this._right) {
            if (essential) {
                throw new ContentCannotBeRemovedException();
            }
            return;
        }
        const outro = this._right;
        this._right = Buffer.alloc(outro.length + buffer.length);
        outro.copy(this._right, 0);
        buffer.copy(this._right, outro.length);
        if (essential) {
            this._assertRight = true;
        }
    }
    prepend(buffer, essential) {
        if (!this._left) {
            if (essential) {
                throw new ContentCannotBeRemovedException();
            }
            return;
        }
        const intro = this._left;
        this._left = Buffer.alloc(intro.length + buffer.length);
        intro.copy(this._left, 0);
        buffer.copy(this._left, intro.length);
        if (essential) {
            this._assertLeft = true;
        }
    }
    assert(left, _content, right) {
        if (left) {
            if (this._assertLeft) {
                throw new ContentCannotBeRemovedException();
            }
        }
        if (right) {
            if (this._assertRight) {
                throw new ContentCannotBeRemovedException();
            }
        }
    }
    remove(left, content, right) {
        if (left) {
            if (this._assertLeft) {
                throw new ContentCannotBeRemovedException();
            }
            this._left = null;
        }
        if (content) {
            this._content = null;
        }
        if (right) {
            if (this._assertRight) {
                throw new ContentCannotBeRemovedException();
            }
            this._right = null;
        }
    }
    copy(target, start) {
        if (this._left) {
            this._left.copy(target, start);
            start += this._left.length;
        }
        if (this._content) {
            this._content.copy(target, start);
            start += this._content.length;
        }
        if (this._right) {
            this._right.copy(target, start);
            start += this._right.length;
        }
        return start;
    }
}
exports.Chunk = Chunk;
/**
 * An utility class that allows buffers to be inserted to the _right or _left, or deleted, while
 * keeping indices to the original buffer.
 *
 * The constructor takes an original buffer, and keeps it into a linked list of chunks, smaller
 * buffers that keep track of _content inserted to the _right or _left of it.
 *
 * Since the Node Buffer structure is non-destructive when slicing, we try to use slicing to create
 * new chunks, and always keep chunks pointing to the original content.
 */
class UpdateBuffer {
    constructor(_originalContent) {
        this._originalContent = _originalContent;
        this._linkedList = new linked_list_1.LinkedList(new Chunk(0, _originalContent.length, _originalContent));
    }
    _assertIndex(index) {
        if (index < 0 || index > this._originalContent.length) {
            throw new IndexOutOfBoundException(index, 0, this._originalContent.length);
        }
    }
    _slice(start) {
        // If start is longer than the content, use start, otherwise determine exact position in string.
        const index = start >= this._originalContent.length ? start : this._getTextPosition(start);
        this._assertIndex(index);
        // Find the chunk by going through the list.
        const h = this._linkedList.find(chunk => index <= chunk.end);
        if (!h) {
            throw Error('Chunk cannot be found.');
        }
        if (index == h.end && h.next !== null) {
            return [h, h.next];
        }
        return [h, h.slice(index)];
    }
    /**
     * Gets the position in the content based on the position in the string.
     * Some characters might be wider than one byte, thus we have to determine the position using
     * string functions.
     */
    _getTextPosition(index) {
        return Buffer.from(this._originalContent.toString().substring(0, index)).length;
    }
    get length() {
        return this._linkedList.reduce((acc, chunk) => acc + chunk.length, 0);
    }
    get original() {
        return this._originalContent;
    }
    toString(encoding = 'utf-8') {
        return this._linkedList.reduce((acc, chunk) => acc + chunk.toString(encoding), '');
    }
    generate() {
        const result = Buffer.allocUnsafe(this.length);
        let i = 0;
        this._linkedList.forEach(chunk => {
            chunk.copy(result, i);
            i += chunk.length;
        });
        return result;
    }
    insertLeft(index, content, assert = false) {
        this._slice(index)[0].append(content, assert);
    }
    insertRight(index, content, assert = false) {
        this._slice(index)[1].prepend(content, assert);
    }
    remove(index, length) {
        const end = index + length;
        const first = this._slice(index)[1];
        const last = this._slice(end)[1];
        let curr;
        for (curr = first; curr && curr !== last; curr = curr.next) {
            curr.assert(curr !== first, curr !== last, curr === first);
        }
        for (curr = first; curr && curr !== last; curr = curr.next) {
            curr.remove(curr !== first, curr !== last, curr === first);
        }
        if (curr) {
            curr.remove(true, false, false);
        }
    }
}
exports.UpdateBuffer = UpdateBuffer;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoidXBkYXRlLWJ1ZmZlci5qcyIsInNvdXJjZVJvb3QiOiIuLyIsInNvdXJjZXMiOlsicGFja2FnZXMvYW5ndWxhcl9kZXZraXQvc2NoZW1hdGljcy9zcmMvdXRpbGl0eS91cGRhdGUtYnVmZmVyLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7O0FBQUE7Ozs7OztHQU1HO0FBQ0gsK0NBQXFEO0FBQ3JELCtDQUEyQztBQUczQyw4QkFBc0MsU0FBUSxvQkFBYTtJQUN6RCxZQUFZLEtBQWEsRUFBRSxHQUFXLEVBQUUsR0FBRyxHQUFHLFFBQVE7UUFDcEQsS0FBSyxDQUFDLFNBQVMsS0FBSyxzQkFBc0IsR0FBRyxLQUFLLEdBQUcsSUFBSSxDQUFDLENBQUM7SUFDN0QsQ0FBQztDQUNGO0FBSkQsNERBSUM7QUFDRCxxQ0FBNkMsU0FBUSxvQkFBYTtJQUNoRTtRQUNFLEtBQUssQ0FBQyx5REFBeUQsQ0FBQyxDQUFDO0lBQ25FLENBQUM7Q0FDRjtBQUpELDBFQUlDO0FBR0Q7Ozs7OztHQU1HO0FBQ0g7SUFVRSxZQUFtQixLQUFhLEVBQVMsR0FBVyxFQUFTLGVBQXVCO1FBQWpFLFVBQUssR0FBTCxLQUFLLENBQVE7UUFBUyxRQUFHLEdBQUgsR0FBRyxDQUFRO1FBQVMsb0JBQWUsR0FBZixlQUFlLENBQVE7UUFSNUUsVUFBSyxHQUFrQixNQUFNLENBQUMsS0FBSyxDQUFDLENBQUMsQ0FBQyxDQUFDO1FBQ3ZDLFdBQU0sR0FBa0IsTUFBTSxDQUFDLEtBQUssQ0FBQyxDQUFDLENBQUMsQ0FBQztRQUV4QyxnQkFBVyxHQUFHLEtBQUssQ0FBQztRQUNwQixpQkFBWSxHQUFHLEtBQUssQ0FBQztRQUU3QixTQUFJLEdBQWlCLElBQUksQ0FBQztRQUd4QixJQUFJLENBQUMsUUFBUSxHQUFHLGVBQWUsQ0FBQyxLQUFLLENBQUMsS0FBSyxFQUFFLEdBQUcsQ0FBQyxDQUFDO0lBQ3BELENBQUM7SUFFRCxJQUFJLE1BQU07UUFDUixPQUFPLENBQUMsSUFBSSxDQUFDLEtBQUssQ0FBQyxD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LElBQUksRUFBRSxJQUFJLEdBQUcsSUFBSSxDQUFDLElBQUksRUFBRTtZQUMxRCxJQUFJLENBQUMsTUFBTSxDQUFDLElBQUksS0FBSyxLQUFLLEVBQUUsSUFBSSxLQUFLLElBQUksRUFBRSxJQUFJLEtBQUssS0FBSyxDQUFDLENBQUM7U0FDNUQ7UUFFRCxJQUFJLElBQUksRUFBRTtZQUNSLElBQUksQ0FBQyxNQUFNLENBQUMsSUFBSSxFQUFFLEtBQUssRUFBRSxLQUFLLENBQUMsQ0FBQztTQUNqQztJQUNILENBQUM7Q0FDRjtBQXZGRCxvQ0F1RkMiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5pbXBvcnQgeyBCYXNlRXhjZXB0aW9uIH0gZnJvbSAnQGFuZ3VsYXItZGV2a2l0L2NvcmUnO1xuaW1wb3J0IHsgTGlua2VkTGlzdCB9IGZyb20gJy4vbGlua2VkLWxpc3QnO1xuXG5cbmV4cG9ydCBjbGFzcyBJbmRleE91dE9mQm91bmRFeGNlcHRpb24gZXh0ZW5kcyBCYXNlRXhjZXB0aW9uIHtcbiAgY29uc3RydWN0b3IoaW5kZXg6IG51bWJlciwgbWluOiBudW1iZXIsIG1heCA9IEluZmluaXR5KSB7XG4gICAgc3VwZXIoYEluZGV4ICR7aW5kZXh9IG91dHNpZGUgb2YgcmFuZ2UgWyR7bWlufSwgJHttYXh9XS5gKTtcbiAgfVxufVxuZXhwb3J0IGNsYXNzIENvbnRlbnRDYW5ub3RCZVJlbW92ZWRFeGNlcHRpb24gZXh0ZW5kcyBCYXNlRXhjZXB0aW9uIHtcbiAgY29uc3RydWN0b3IoKSB7XG4gICAgc3VwZXIoYFVzZXIgdHJpZWQgdG8gcmVtb3ZlIGNvbnRlbnQgdGhhdCB3YXMgbWFya2VkIGVzc2VudGlhbC5gKTtcbiAgfVxufVxuXG5cbi8qKlxuICogQSBDaHVuayBkZXNjcmlwdGlvbiwgaW5jbHVkaW5nIGxlZnQvcmlnaHQgY29udGVudCB0aGF0IGhhcyBiZWVuIGluc2VydGVkLlxuICogSWYgX2xlZnQvX3JpZ2h0IGlzIG51bGwsIHRoaXMgbWVhbnMgdGhhdCBjb250ZW50IHdhcyBkZWxldGVkLiBJZiB0aGUgX2NvbnRlbnQgaXMgbnVsbCxcbiAqIGl0IG1lYW5zIHRoZSBjb250ZW50IGl0c2VsZiB3YXMgZGVsZXRlZC5cbiAqXG4gKiBAc2VlIFVwZGF0ZUJ1ZmZlclxuICovXG5leHBvcnQgY2xhc3MgQ2h1bmsge1xuICBwcml2YXRlIF9jb250ZW50OiBCdWZmZXIgfCBudWxsO1xuICBwcml2YXRlIF9sZWZ0OiBCdWZmZXIgfCBudWxsID0gQnVmZmVyLmFsbG9jKDApO1xuICBwcml2YXRlIF9yaWdodDogQnVmZmVyIHwgbnVsbCA9IEJ1ZmZlci5hbGxvYygwKTtcblxuICBwcml2YXRlIF9hc3NlcnRMZWZ0ID0gZmFsc2U7XG4gIHByaXZhdGUgX2Fzc2VydFJpZ2h0ID0gZmFsc2U7XG5cbiAgbmV4dDogQ2h1bmsgfCBudWxsID0gbnVsbDtcblxuICBjb25zdHJ1Y3RvcihwdWJsaWMgc3RhcnQ6IG51bWJlciwgcHVibGljIGVuZDogbnVtYmVyLCBwdWJsaWMgb3JpZ2luYWxDb250ZW50OiBCdWZmZXIpIHtcbiAgICB0aGlzLl9jb250ZW50ID0gb3JpZ2luYWxDb250ZW50LnNsaWNlKHN0YXJ0LCBlbmQpO1xuICB9XG5cbiAgZ2V0IGxlbmd0aCgpIHtcbiAgICByZXR1cm4gKHRoaXMuX2xlZnQgPyB0aGlzLl9sZWZ0Lmxlbmd0aCA6IDApXG4gICAgICAgICArICh0aGlzLl9jb250ZW50ID8gdGhpcy5fY29udGVudC5sZW5ndGggOiAwKVxuICAgICAgICAgKyAodGhpcy5fcmlnaHQgPyB0aGlzLl9yaWdodC5sZW5ndGggOiAwKTtcbiAgfVxuICB0b1N0cmluZyhlbmNvZGluZyA9ICd1dGYtOCcpIHtcbiAgICByZXR1cm4gKHRoaXMuX2xlZnQgPyB0aGlzLl9sZWZ0LnRvU3RyaW5nKGVuY29kaW5nKSA6ICcnKVxuICAgICAgICAgKyAodGhpcy5fY29udGVudCA/IHRoaXMuX2NvbnRlbnQudG9TdHJpbmcoZW5jb2RpbmcpIDogJycpXG4gICAgICAgICArICh0aGlzLl9yaWdodCA/IHRoaXMuX3JpZ2h0LnRvU3RyaW5nKGVuY29kaW5nKSA6ICcnKTtcbiAgfVxuXG4gIHNsaWNlKHN0YXJ0OiBudW1iZXIpIHtcbiAgICBpZiAoc3RhcnQgPCB0aGlzLnN0YXJ0IHx8IHN0YXJ0ID4gdGhpcy5lbmQpIHtcbiAgICAgIHRocm93IG5ldyBJbmRleE91dE9mQm91bmRFeGNlcHRpb24oc3RhcnQsIHRoaXMuc3RhcnQsIHRoaXMuZW5kKTtcbiAgICB9XG5cbiAgICAvLyBVcGRhdGUgX2NvbnRlbnQgdG8gdGhlIG5ldyBpbmRpY2VzLlxuICAgIGNvbnN0IG5ld0NodW5rID0gbmV3IENodW5rKHN0YXJ0LCB0aGlzLmVuZCwgdGhpcy5vcmlnaW5hbENvbnRlbnQpO1xuXG4gICAgLy8gSWYgdGhpcyBjaHVuayBoYXMgX2NvbnRlbnQsIHJlc2xpY2UgdGhlIG9yaWdpbmFsIF9jb250ZW50LiBXZSBtb3ZlIHRoZSBfcmlnaHQgc28gd2UgYXJlIG5vdFxuICAgIC8vIGxvc2luZyBhbnkgZGF0YSBoZXJlLiBJZiB0aGlzIGNodW5rIGhhcyBiZWVuIGRlbGV0ZWQsIHRoZSBuZXh0IGNodW5rIHNob3VsZCBhbHNvIGJlIGRlbGV0ZWQuXG4gICAgaWYgKHRoaXMuX2NvbnRlbnQpIHtcbiAgICAgIHRoaXMuX2NvbnRlbnQgPSB0aGlzLm9yaWdpbmFsQ29udGVudC5zbGljZSh0aGlzLnN0YXJ0LCBzdGFydCk7XG4gICAgfSBlbHNlIHtcbiAgICAgIG5ld0NodW5rLl9jb250ZW50ID0gdGhpcy5fY29udGVudDtcbiAgICAgIGlmICh0aGlzLl9yaWdodCA9PT0gbnVsbCkge1xuICAgICAgICBuZXdDaHVuay5fbGVmdCA9IG51bGw7XG4gICAgICB9XG4gICAgfVxuICAgIHRoaXMuZW5kID0gc3RhcnQ7XG5cbiAgICAvLyBNb3ZlIF9yaWdodCB0byB0aGUgbmV3IGNodW5rLlxuICAgIG5ld0NodW5rLl9yaWdodCA9IHRoaXMuX3JpZ2h0O1xuICAgIHRoaXMuX3JpZ2h0ID0gdGhpcy5fcmlnaHQgJiYgQnVmZmVyLmFsbG9jKDApO1xuXG4gICAgLy8gVXBkYXRlIGVzc2VudGlhbHMuXG4gICAgaWYgKHRoaXMuX2Fzc2VydFJpZ2h0KSB7XG4gICAgICBuZXdDaHVuay5fYXNzZXJ0UmlnaHQgPSB0cnVlO1xuICAgICAgdGhpcy5fYXNzZXJ0UmlnaHQgPSBmYWxzZTtcbiAgICB9XG5cbiAgICAvLyBVcGRhdGUgdGhlIGxpbmtlZCBsaXN0LlxuICAgIG5ld0NodW5rLm5leHQgPSB0aGlzLm5leHQ7XG4gICAgdGhpcy5uZXh0ID0gbmV3Q2h1bms7XG5cbiAgICByZXR1cm4gbmV3Q2h1bms7XG4gIH1cblxuICBhcHBlbmQoYnVmZmVyOiBCdWZmZXIsIGVzc2VudGlhbDogYm9vbGVhbikge1xuICAgIGlmICghdGhpcy5fcmlnaHQpIHtcbiAgICAgIGlmIChlc3NlbnRpYWwpIHtcbiAgICAgICAgdGhyb3cgbmV3IENvbnRlbnRDYW5ub3RCZVJlbW92ZWRFeGNlcHRpb24oKTtcbiAgICAgIH1cblxuICAgICAgcmV0dXJuO1xuICAgIH1cblxuICAgIGNvbnN0IG91dHJvID0gdGhpcy5fcmlnaHQ7XG4gICAgdGhpcy5fcmlnaHQgPSBCdWZmZXIuYWxsb2Mob3V0cm8ubGVuZ3RoICsgYnVmZmVyLmxlbmd0aCk7XG4gICAgb3V0cm8uY29weSh0aGlzLl9yaWdodCwgMCk7XG4gICAgYnVmZmVyLmNvcHkodGhpcy5fcmlnaHQsIG91dHJvLmxlbmd0aCk7XG5cbiAgICBpZiAoZXNzZW50aWFsKSB7XG4gICAgICB0aGlzLl9hc3NlcnRSaWdodCA9IHRydWU7XG4gICAgfVxuICB9XG4gIHByZXBlbmQoYnVmZmVyOiBCdWZmZXIsIGVzc2VudGlhbDogYm9vbGVhbikge1xuICAgIGlmICghdGhpcy5fbGVmdCkge1xuICAgICAgaWYgKGVzc2VudGlhbCkge1xuICAgICAgICB0aHJvdyBuZXcgQ29udGVudENhbm5vdEJlUmVtb3ZlZEV4Y2VwdGlvbigpO1xuICAgICAgfVxuXG4gICAgICByZXR1cm47XG4gICAgfVxuXG4gICAgY29uc3QgaW50cm8gPSB0aGlzLl9sZWZ0O1xuICAgIHRoaXMuX2xlZnQgPSBCdWZmZXIuYWxsb2MoaW50cm8ubGVuZ3RoICsgYnVmZmVyLmxlbmd0aCk7XG4gICAgaW50cm8uY29weSh0aGlzLl9sZWZ0LCAwKTtcbiAgICBidWZmZXIuY29weSh0aGlzLl9sZWZ0LCBpbnRyby5sZW5ndGgpO1xuXG4gICAgaWYgKGVzc2VudGlhbCkge1xuICAgICAgdGhpcy5fYXNzZXJ0TGVmdCA9IHRydWU7XG4gICAgfVxuICB9XG5cbiAgYXNzZXJ0KGxlZnQ6IGJvb2xlYW4sIF9jb250ZW50OiBib29sZWFuLCByaWdodDogYm9vbGVhbikge1xuICAgIGlmIChsZWZ0KSB7XG4gICAgICBpZiAodGhpcy5fYXNzZXJ0TGVmdCkge1xuICAgICAgICB0aHJvdyBuZXcgQ29udGVudENhbm5vdEJlUmVtb3ZlZEV4Y2VwdGlvbigpO1xuICAgICAgfVxuICAgIH1cbiAgICBpZiAocmlnaHQpIHtcbiAgICAgIGlmICh0aGlzLl9hc3NlcnRSaWdodCkge1xuICAgICAgICB0aHJvdyBuZXcgQ29udGVudENhbm5vdEJlUmVtb3ZlZEV4Y2VwdGlvbigpO1xuICAgICAgfVxuICAgIH1cbiAgfVxuXG4gIHJlbW92ZShsZWZ0OiBib29sZWFuLCBjb250ZW50OiBib29sZWFuLCByaWdodDogYm9vbGVhbikge1xuICAgIGlmIChsZWZ0KSB7XG4gICAgICBpZiAodGhpcy5fYXNzZXJ0TGVmdCkge1xuICAgICAgICB0aHJvdyBuZXcgQ29udGVudENhbm5vdEJlUmVtb3ZlZEV4Y2VwdGlvbigpO1xuICAgICAgfVxuICAgICAgdGhpcy5fbGVmdCA9IG51bGw7XG4gICAgfVxuICAgIGlmIChjb250ZW50KSB7XG4gICAgICB0aGlzLl9jb250ZW50ID0gbnVsbDtcbiAgICB9XG4gICAgaWYgKHJpZ2h0KSB7XG4gICAgICBpZiAodGhpcy5fYXNzZXJ0UmlnaHQpIHtcbiAgICAgICAgdGhyb3cgbmV3IENvbnRlbnRDYW5ub3RCZVJlbW92ZWRFeGNlcHRpb24oKTtcbiAgICAgIH1cbiAgICAgIHRoaXMuX3JpZ2h0ID0gbnVsbDtcbiAgICB9XG4gIH1cblxuICBjb3B5KHRhcmdldDogQnVmZmVyLCBzdGFydDogbnVtYmVyKSB7XG4gICAgaWYgKHRoaXMuX2xlZnQpIHtcbiAgICAgIHRoaXMuX2xlZnQuY29weSh0YXJnZXQsIHN0YXJ0KTtcbiAgICAgIHN0YXJ0ICs9IHRoaXMuX2xlZnQubGVuZ3RoO1xuICAgIH1cbiAgICBpZiAodGhpcy5fY29udGVudCkge1xuICAgICAgdGhpcy5fY29udGVudC5jb3B5KHRhcmdldCwgc3RhcnQpO1xuICAgICAgc3RhcnQgKz0gdGhpcy5fY29udGVudC5sZW5ndGg7XG4gICAgfVxuICAgIGlmICh0aGlzLl9yaWdodCkge1xuICAgICAgdGhpcy5fcmlnaHQuY29weSh0YXJnZXQsIHN0YXJ0KTtcbiAgICAgIHN0YXJ0ICs9IHRoaXMuX3JpZ2h0Lmxlbmd0aDtcbiAgICB9XG5cbiAgICByZXR1cm4gc3RhcnQ7XG4gIH1cbn1cblxuXG4vKipcbiAqIEFuIHV0aWxpdHkgY2xhc3MgdGhhdCBhbGxvd3MgYnVmZmVycyB0byBiZSBpbnNlcnRlZCB0byB0aGUgX3JpZ2h0IG9yIF9sZWZ0LCBvciBkZWxldGVkLCB3aGlsZVxuICoga2VlcGluZyBpbmRpY2VzIHRvIHRoZSBvcmlnaW5hbCBidWZmZXIuXG4gKlxuICogVGhlIGNvbnN0cnVjdG9yIHRha2VzIGFuIG9yaWdpbmFsIGJ1ZmZlciwgYW5kIGtlZXBzIGl0IGludG8gYSBsaW5rZWQgbGlzdCBvZiBjaHVua3MsIHNtYWxsZXJcbiAqIGJ1ZmZlcnMgdGhhdCBrZWVwIHRyYWNrIG9mIF9jb250ZW50IGluc2VydGVkIHRvIHRoZSBfcmlnaHQgb3IgX2xlZnQgb2YgaXQuXG4gKlxuICogU2luY2UgdGhlIE5vZGUgQnVmZmVyIHN0cnVjdHVyZSBpcyBub24tZGVzdHJ1Y3RpdmUgd2hlbiBzbGljaW5nLCB3ZSB0cnkgdG8gdXNlIHNsaWNpbmcgdG8gY3JlYXRlXG4gKiBuZXcgY2h1bmtzLCBhbmQgYWx3YXlzIGtlZXAgY2h1bmtzIHBvaW50aW5nIHRvIHRoZSBvcmlnaW5hbCBjb250ZW50LlxuICovXG5leHBvcnQgY2xhc3MgVXBkYXRlQnVmZmVyIHtcbiAgcHJvdGVjdGVkIF9saW5rZWRMaXN0OiBMaW5rZWRMaXN0PENodW5rPjtcblxuICBjb25zdHJ1Y3Rvcihwcm90ZWN0ZWQgX29yaWdpbmFsQ29udGVudDogQnVmZmVyKSB7XG4gICAgdGhpcy5fbGlua2VkTGlzdCA9IG5ldyBMaW5rZWRMaXN0KG5ldyBDaHVuaygwLCBfb3JpZ2luYWxDb250ZW50Lmxlbmd0aCwgX29yaWdpbmFsQ29udGVudCkpO1xuICB9XG5cbiAgcHJvdGVjdGVkIF9hc3NlcnRJbmRleChpbmRleDogbnVtYmVyKSB7XG4gICAgaWYgKGluZGV4IDwgMCB8fCBpbmRleCA+IHRoaXMuX29yaWdpbmFsQ29udGVudC5sZW5ndGgpIHtcbiAgICAgIHRocm93IG5ldyBJbmRleE91dE9mQm91bmRFeGNlcHRpb24oaW5kZXgsIDAsIHRoaXMuX29yaWdpbmFsQ29udGVudC5sZW5ndGgpO1xuICAgIH1cbiAgfVxuXG4gIHByb3RlY3RlZCBfc2xpY2Uoc3RhcnQ6IG51bWJlcik6IFtDaHVuaywgQ2h1bmtdIHtcbiAgICAvLyBJZiBzdGFydCBpcyBsb25nZXIgdGhhbiB0aGUgY29udGVudCwgdXNlIHN0YXJ0LCBvdGhlcndpc2UgZGV0ZXJtaW5lIGV4YWN0IHBvc2l0aW9uIGluIHN0cmluZy5cbiAgICBjb25zdCBpbmRleCA9IHN0YXJ0ID49IHRoaXMuX29yaWdpbmFsQ29udGVudC5sZW5ndGggPyBzdGFydCA6IHRoaXMuX2dldFRleHRQb3NpdGlvbihzdGFydCk7XG5cbiAgICB0aGlzLl9hc3NlcnRJbmRleChpbmRleCk7XG5cbiAgICAvLyBGaW5kIHRoZSBjaHVuayBieSBnb2luZyB0aHJvdWdoIHRoZSBsaXN0LlxuICAgIGNvbnN0IGggPSB0aGlzLl9saW5rZWRMaXN0LmZpbmQoY2h1bmsgPT4gaW5kZXggPD0gY2h1bmsuZW5kKTtcbiAgICBpZiAoIWgpIHtcbiAgICAgIHRocm93IEVycm9yKCdDaHVuayBjYW5ub3QgYmUgZm91bmQuJyk7XG4gICAgfVxuXG4gICAgaWYgKGluZGV4ID09IGguZW5kICYmIGgubmV4dCAhPT0gbnVsbCkge1xuICAgICAgcmV0dXJuIFtoLCBoLm5leHRdO1xuICAgIH1cblxuICAgIHJldHVybiBbaCwgaC5zbGljZShpbmRleCldO1xuICB9XG5cbiAgLyoqXG4gICAqIEdldHMgdGhlIHBvc2l0aW9uIGluIHRoZSBjb250ZW50IGJhc2VkIG9uIHRoZSBwb3NpdGlvbiBpbiB0aGUgc3RyaW5nLlxuICAgKiBTb21lIGNoYXJhY3RlcnMgbWlnaHQgYmUgd2lkZXIgdGhhbiBvbmUgYnl0ZSwgdGh1cyB3ZSBoYXZlIHRvIGRldGVybWluZSB0aGUgcG9zaXRpb24gdXNpbmdcbiAgICogc3RyaW5nIGZ1bmN0aW9ucy5cbiAgICovXG4gIHByb3RlY3RlZCBfZ2V0VGV4dFBvc2l0aW9uKGluZGV4OiBudW1iZXIpOiBudW1iZXIge1xuICAgIHJldHVybiBCdWZmZXIuZnJvbSh0aGlzLl9vcmlnaW5hbENvbnRlbnQudG9TdHJpbmcoKS5zdWJzdHJpbmcoMCwgaW5kZXgpKS5sZW5ndGg7XG4gIH1cblxuICBnZXQgbGVuZ3RoKCk6IG51bWJlciB7XG4gICAgcmV0dXJuIHRoaXMuX2xpbmtlZExpc3QucmVkdWNlKChhY2MsIGNodW5rKSA9PiBhY2MgKyBjaHVuay5sZW5ndGgsIDApO1xuICB9XG4gIGdldCBvcmlnaW5hbCgpOiBCdWZmZXIge1xuICAgIHJldHVybiB0aGlzLl9vcmlnaW5hbENvbnRlbnQ7XG4gIH1cblxuICB0b1N0cmluZyhlbmNvZGluZyA9ICd1dGYtOCcpOiBzdHJpbmcge1xuICAgIHJldHVybiB0aGlzLl9saW5rZWRMaXN0LnJlZHVjZSgoYWNjLCBjaHVuaykgPT4gYWNjICsgY2h1bmsudG9TdHJpbmcoZW5jb2RpbmcpLCAnJyk7XG4gIH1cbiAgZ2VuZXJhdGUoKTogQnVmZmVyIHtcbiAgICBjb25zdCByZXN1bHQgPSBCdWZmZXIuYWxsb2NVbnNhZmUodGhpcy5sZW5ndGgpO1xuICAgIGxldCBpID0gMDtcbiAgICB0aGlzLl9saW5rZWRMaXN0LmZvckVhY2goY2h1bmsgPT4ge1xuICAgICAgY2h1bmsuY29weShyZXN1bHQsIGkpO1xuICAgICAgaSArPSBjaHVuay5sZW5ndGg7XG4gICAgfSk7XG5cbiAgICByZXR1cm4gcmVzdWx0O1xuICB9XG5cbiAgaW5zZXJ0TGVmdChpbmRleDogbnVtYmVyLCBjb250ZW50OiBCdWZmZXIsIGFzc2VydCA9IGZhbHNlKSB7XG4gICAgdGhpcy5fc2xpY2UoaW5kZXgpWzBdLmFwcGVuZChjb250ZW50LCBhc3NlcnQpO1xuICB9XG4gIGluc2VydFJpZ2h0KGluZGV4OiBudW1iZXIsIGNvbnRlbnQ6IEJ1ZmZlciwgYXNzZXJ0ID0gZmFsc2UpIHtcbiAgICB0aGlzLl9zbGljZShpbmRleClbMV0ucHJlcGVuZChjb250ZW50LCBhc3NlcnQpO1xuICB9XG5cbiAgcmVtb3ZlKGluZGV4OiBudW1iZXIsIGxlbmd0aDogbnVtYmVyKSB7XG4gICAgY29uc3QgZW5kID0gaW5kZXggKyBsZW5ndGg7XG5cbiAgICBjb25zdCBmaXJzdCA9IHRoaXMuX3NsaWNlKGluZGV4KVsxXTtcbiAgICBjb25zdCBsYXN0ID0gdGhpcy5fc2xpY2UoZW5kKVsxXTtcblxuICAgIGxldCBjdXJyOiBDaHVuayB8IG51bGw7XG4gICAgZm9yIChjdXJyID0gZmlyc3Q7IGN1cnIgJiYgY3VyciAhPT0gbGFzdDsgY3VyciA9IGN1cnIubmV4dCkge1xuICAgICAgY3Vyci5hc3NlcnQoY3VyciAhPT0gZmlyc3QsIGN1cnIgIT09IGxhc3QsIGN1cnIgPT09IGZpcnN0KTtcbiAgICB9XG4gICAgZm9yIChjdXJyID0gZmlyc3Q7IGN1cnIgJiYgY3VyciAhPT0gbGFzdDsgY3VyciA9IGN1cnIubmV4dCkge1xuICAgICAgY3Vyci5yZW1vdmUoY3VyciAhPT0gZmlyc3QsIGN1cnIgIT09IGxhc3QsIGN1cnIgPT09IGZpcnN0KTtcbiAgICB9XG5cbiAgICBpZiAoY3Vycikge1xuICAgICAgY3Vyci5yZW1vdmUodHJ1ZSwgZmFsc2UsIGZhbHNlKTtcbiAgICB9XG4gIH1cbn1cbiJdfQ==