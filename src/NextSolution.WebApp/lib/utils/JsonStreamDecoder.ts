// source: https://gist.github.com/markotny/d21ef4e1af3d6ea5332b948c9c9987e5
export class JsonStreamDecoder {
  /** item starts and ends at level 0 */
  private level = 0;

  /** when an item is split in two */
  private partialItem = "";

  private decoder = new TextDecoder();

  public decodeChunk<T>(value: Uint8Array, decodedItemCallback: (item: T) => void): void {
    const chunk = this.decoder.decode(value);
    let itemStart = 0;

    for (let i = 0; i < chunk.length; i++) {
      if (chunk[i] === JTOKEN_START_OBJECT) {
        if (this.level === 0) {
          itemStart = i;
        }
        this.level++;
      }
      if (chunk[i] === JTOKEN_END_OBJECT) {
        this.level--;
        if (this.level === 0) {
          let item = chunk.substring(itemStart, i + 1);
          if (this.partialItem) {
            item = this.partialItem + item;
            this.partialItem = "";
          }
          decodedItemCallback(JSON.parse(item));
        }
      }
    }
    if (this.level !== 0) {
      this.partialItem = chunk.substring(itemStart);
    }
  }
}
const JTOKEN_START_OBJECT = "{";
const JTOKEN_END_OBJECT = "}";
