declare module 'heic-decode' {
  type DecodeResult = {
    width: number
    height: number
    data: Uint8ClampedArray
  }

  export default function decode(input: { buffer: ArrayBuffer | Uint8Array }): Promise<DecodeResult>
}
